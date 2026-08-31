using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Nop.Data;
using Nop.Services.Logging;
using Widgets.FirebasePushNotification.Domain;

namespace Widgets.FirebasePushNotification.Services;

public class FirebaseNotificationService : IFirebaseNotificationService
{
	private readonly ILogger _logger;

	private readonly IRepository<FirebaseDeviceToken> _tokenRepository;

	public FirebaseNotificationService(ILogger logger, IRepository<FirebaseDeviceToken> tokenRepository)
	{
		_logger = logger;
		_tokenRepository = tokenRepository;
	}

	private static void Log(string message)
	{
		Console.WriteLine("[FCM] " + message);
	}

	public async Task<bool> SubscribeDeviceAsync(int customerId, string token, string platform)
	{
		if (customerId <= 0 || string.IsNullOrWhiteSpace(token) || !IsAllowedPlatform(platform))
		{
			return false;
		}
		string normalizedPlatform = platform.Trim().ToLowerInvariant();
		string normalizedToken = token.Trim();
		DateTime now = DateTime.UtcNow;
		FirebaseDeviceToken existing = await _tokenRepository.Table.FirstOrDefaultAsync((FirebaseDeviceToken x) => x.Token == normalizedToken);
		if (existing == null)
		{
			await _tokenRepository.InsertAsync(new FirebaseDeviceToken
			{
				CustomerId = customerId,
				Token = normalizedToken,
				Platform = normalizedPlatform,
				IsActive = true,
				CreatedOnUtc = now,
				UpdatedOnUtc = now,
				LastUsedOnUtc = now
			});
			return true;
		}
		existing.CustomerId = customerId;
		existing.Platform = normalizedPlatform;
		existing.IsActive = true;
		existing.UpdatedOnUtc = now;
		existing.LastUsedOnUtc = now;
		await _tokenRepository.UpdateAsync(existing);
		return true;
	}

	public async Task<bool> UnsubscribeDeviceAsync(int customerId, string token)
	{
		if (customerId <= 0 || string.IsNullOrWhiteSpace(token))
		{
			return false;
		}
		string normalizedToken = token.Trim();
		FirebaseDeviceToken existing = await _tokenRepository.Table.FirstOrDefaultAsync((FirebaseDeviceToken x) => x.CustomerId == customerId && x.Token == normalizedToken);
		if (existing == null)
		{
			return true;
		}
		existing.IsActive = false;
		existing.UpdatedOnUtc = DateTime.UtcNow;
		await _tokenRepository.UpdateAsync(existing);
		return true;
	}

	public async Task<bool> EnsureCustomerTokenAsync(int customerId, string platform = "web")
	{
		if (customerId <= 0)
		{
			return false;
		}
		string normalizedPlatform = (IsAllowedPlatform(platform) ? platform.Trim().ToLowerInvariant() : "web");
		DateTime now = DateTime.UtcNow;
		FirebaseDeviceToken existingToken = await (from x in _tokenRepository.Table
			where x.CustomerId == customerId
			orderby x.Id descending
			select x).FirstOrDefaultAsync();
		if (existingToken != null)
		{
			if (!existingToken.IsActive)
			{
				existingToken.IsActive = true;
				existingToken.Platform = (string.IsNullOrWhiteSpace(existingToken.Platform) ? normalizedPlatform : existingToken.Platform);
				existingToken.UpdatedOnUtc = now;
				existingToken.LastUsedOnUtc = now;
				await _tokenRepository.UpdateAsync(existingToken);
			}
			return true;
		}
		await _tokenRepository.InsertAsync(new FirebaseDeviceToken
		{
			CustomerId = customerId,
			Token = "",
			Platform = normalizedPlatform,
			IsActive = true,
			CreatedOnUtc = now,
			UpdatedOnUtc = now,
			LastUsedOnUtc = now
		});
		return true;
	}

	public async Task<bool> SendNotificationAsync(int customerId, string title, string body, Dictionary<string, string>? data = null, string platform = "all")
	{
		if (customerId <= 0)
		{
			return false;
		}
		string normalizedPlatform = NormalizePlatform(platform);
		IQueryable<FirebaseDeviceToken> tokensQuery = _tokenRepository.Table.Where((FirebaseDeviceToken x) => x.CustomerId == customerId && x.IsActive && x.Token != "");
		if (normalizedPlatform != "all")
		{
			tokensQuery = tokensQuery.Where((FirebaseDeviceToken x) => x.Platform == normalizedPlatform);
		}
		List<string> deviceTokens = await tokensQuery.Select((FirebaseDeviceToken x) => x.Token).Distinct().ToListAsync();
		Log($"SendNotification customerId={customerId}, platform={normalizedPlatform}, tokens found={deviceTokens.Count}");
		if (!deviceTokens.Any())
		{
			Log($"No active tokens for customerId={customerId}");
			return false;
		}
		bool sent = false;
		foreach (string deviceToken in deviceTokens)
		{
			Log("Sending to token");
			if (await SendToDeviceTokenAsync(title, body, deviceToken, data, normalizedPlatform))
			{
				sent = true;
			}
		}
		return sent;
	}

	public async Task<int> SendNotificationToManyAsync(IList<int> customerIds, string title, string body, Dictionary<string, string>? data = null, string platform = "all")
	{
		if (customerIds == null || customerIds.Count == 0)
		{
			return 0;
		}
		try
		{
			string normalizedPlatform = NormalizePlatform(platform);
			IQueryable<FirebaseDeviceToken> tokensQuery = _tokenRepository.Table.Where((FirebaseDeviceToken x) => customerIds.Contains(x.CustomerId) && x.IsActive && x.Token != "");
			if (normalizedPlatform != "all")
			{
				tokensQuery = tokensQuery.Where((FirebaseDeviceToken x) => x.Platform == normalizedPlatform);
			}
			List<FirebaseDeviceToken> tokens = await tokensQuery.ToListAsync();
			if (!tokens.Any())
			{
				return 0;
			}
			List<string> uniqueDeviceTokens = (from x in tokens
				select x.Token into x
				where !string.IsNullOrWhiteSpace(x)
				select x).Distinct().ToList();
			if (!uniqueDeviceTokens.Any())
			{
				return 0;
			}
			return await SendToManyDeviceTokensAsync(uniqueDeviceTokens, title, body, data, normalizedPlatform);
		}
		catch (Exception exception)
		{
			await _logger.ErrorAsync("Error while sending FCM notifications", exception);
			return 0;
		}
	}

	private async Task<bool> SendToDeviceTokenAsync(string title, string body, string deviceToken, Dictionary<string, string>? data = null, string platform = "all", CancellationToken cancellationToken = default(CancellationToken))
	{
		Dictionary<string, string> messageData = ((data != null) ? new Dictionary<string, string>(data) : new Dictionary<string, string>());
		messageData["title"] = title;
		messageData["body"] = body;
		Message message = new Message
		{
			Token = deviceToken,
			Notification = new Notification
			{
				Title = title,
				Body = body
			},
			Data = messageData
		};
		try
		{
			Log("Message sent successfully, messageId=" + await FirebaseMessaging.DefaultInstance.SendAsync(message));
		}
		catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered || ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
		{
			Log($"Token invalid/unregistered: {ex.MessagingErrorCode} - {ex.Message}");
			await DeactivateTokensAsync(new List<string> { deviceToken });
			return false;
		}
		catch (Exception ex2)
		{
			Log("SendToDevice FAILED: " + ex2.Message);
			return false;
		}
		return true;
	}

	private async Task<int> SendToManyDeviceTokensAsync(IList<string> deviceTokens, string title, string body, Dictionary<string, string>? data = null, string platform = "all", CancellationToken cancellationToken = default(CancellationToken))
	{
		if (deviceTokens == null || deviceTokens.Count == 0)
		{
			return 0;
		}
		Dictionary<string, string> messageData = ((data != null) ? new Dictionary<string, string>(data) : new Dictionary<string, string>());
		messageData["title"] = title;
		messageData["body"] = body;
		MulticastMessage message = new MulticastMessage
		{
			Tokens = deviceTokens.ToList(),
			Notification = new Notification
			{
				Title = title,
				Body = body
			},
			Data = messageData
		};
		Log($"SendMulticast: {deviceTokens.Count} tokens, platform={platform}");
		BatchResponse response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
		Log($"SendMulticast result: success={response.SuccessCount}, failure={response.FailureCount}");
		List<string> invalidTokens = new List<string>();
		for (int i = 0; i < response.Responses.Count; i++)
		{
			if (!response.Responses[i].IsSuccess)
			{
				bool flag;
				switch (response.Responses[i].Exception?.MessagingErrorCode)
				{
				case MessagingErrorCode.InvalidArgument:
				case MessagingErrorCode.Unregistered:
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					invalidTokens.Add(deviceTokens[i]);
				}
			}
		}
		if (invalidTokens.Count > 0)
		{
			await DeactivateTokensAsync(invalidTokens);
		}
		return response.SuccessCount;
	}

	private async Task DeactivateTokensAsync(IList<string> deviceTokens)
	{
		if (deviceTokens.Count == 0)
		{
			return;
		}
		DateTime now = DateTime.UtcNow;
		List<FirebaseDeviceToken> tokens = await _tokenRepository.Table.Where((FirebaseDeviceToken x) => deviceTokens.Contains(x.Token) && x.IsActive).ToListAsync();
		foreach (FirebaseDeviceToken token in tokens)
		{
			token.IsActive = false;
			token.UpdatedOnUtc = now;
		}
		if (tokens.Count > 0)
		{
			await _tokenRepository.UpdateAsync(tokens);
		}
	}

	protected virtual bool IsAllowedPlatform(string platform)
	{
		switch (NormalizePlatform(platform))
		{
		case "android":
		case "ios":
		case "web":
			return true;
		default:
			return false;
		}
	}

	protected virtual string NormalizePlatform(string platform)
	{
		string text = (platform ?? "all").Trim().ToLowerInvariant();
		bool flag;
		switch (text)
		{
		case "android":
		case "ios":
		case "web":
		case "all":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		return flag ? text : "all";
	}
}
