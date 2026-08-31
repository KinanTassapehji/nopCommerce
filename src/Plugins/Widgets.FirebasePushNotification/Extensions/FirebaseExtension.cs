using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection;

namespace Widgets.FirebasePushNotification.Extensions;

public static class FirebaseExtension
{
	public static void AddFirebase(this IServiceCollection services)
	{
		if (FirebaseApp.DefaultInstance != null)
			return;

		var credential = LoadCredential();

		//credentials are a deployment secret and are absent in some environments
		//(CI, a fresh clone); skip Firebase setup rather than failing app startup
		if (credential == null)
			return;

		FirebaseApp.Create(new AppOptions { Credential = credential });
	}

	/// <summary>
	/// Resolves the Firebase service account, explicit sources first.
	/// </summary>
	/// <remarks>
	/// Application-default credentials are deliberately last. They also pick up
	/// "gcloud auth application-default login", which is a developer's personal
	/// Google account rather than this project's service account, and letting
	/// that win would send pushes against the wrong Firebase project.
	/// </remarks>
	private static GoogleCredential LoadCredential()
	{
		var fromEnvironment = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
		if (!string.IsNullOrWhiteSpace(fromEnvironment) && File.Exists(fromEnvironment))
			return GoogleCredential.FromFile(fromEnvironment);

		//Drop the service-account JSON next to the app and it is found by shape
		//rather than by an exact name, so the file Firebase generates (its name
		//embeds the project id) can be used exactly as downloaded.
		var file = FindCredentialFile(Directory.GetCurrentDirectory())
			?? FindCredentialFile(AppContext.BaseDirectory);

		if (file != null)
			return GoogleCredential.FromFile(file);

		//Hosted on GCP: the metadata server supplies the credential.
		try
		{
			return GoogleCredential.GetApplicationDefault();
		}
		catch (Exception)
		{
			return null;
		}
	}

	private static string FindCredentialFile(string directory)
	{
		if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
			return null;

		return Directory.EnumerateFiles(directory, "*firebase-adminsdk*.json").FirstOrDefault();
	}
}
