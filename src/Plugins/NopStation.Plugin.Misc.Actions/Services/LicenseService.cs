using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Logging;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Domains;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Services.Cache;

namespace NopStation.Plugin.Misc.Core.Services;

public class LicenseService : ILicenseService
{
	public static class Constants
	{
		public static string LicenseKeySeed = "22cerfdZX8Uq9LrLHHhYssVD";
	}

	private class DecryptedLicense
	{
		public int[] NopVersion { get; set; }

		public string Domain { get; set; }

		public bool IncludesSubdomains { get; set; }

		public bool SkipCheckDomain { get; set; }

		public bool SkipCheckFileName { get; set; }

		public IList<string> FileNames { get; set; }

		public DecryptedLicense()
		{
			FileNames = new List<string>();
		}
	}

	private bool? _cachedLicensed;

	private readonly ILogger _logger;

	private readonly IHttpContextAccessor _httpContextAccessor;

	private readonly IEncryptionService _encryptionService;

	private readonly NopStationCoreSettings _coreSettings;

	private readonly IStoreContext _storeContext;

	private readonly IRepository<License> _licenseRepository;

	private readonly IStaticCacheManager _cacheManager;

	private readonly SecuritySettings _securitySettings;

	public LicenseService(ILogger logger, IHttpContextAccessor httpContextAccessor, IEncryptionService encryptionService, NopStationCoreSettings coreSettings, IStoreContext storeContext, IRepository<License> licenseRepository, IStaticCacheManager cacheManager, SecuritySettings securitySettings)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_encryptionService = encryptionService;
		_coreSettings = coreSettings;
		_storeContext = storeContext;
		_licenseRepository = licenseRepository;
		_cacheManager = cacheManager;
		_securitySettings = securitySettings;
	}

	private string DecryptTextFromMemory(byte[] data, byte[] key, byte[] iv)
	{
		using MemoryStream stream = new MemoryStream(data);
		using CryptoStream stream2 = new CryptoStream(stream, TripleDES.Create().CreateDecryptor(key, iv), CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2, Encoding.Unicode);
		return streamReader.ReadToEnd();
	}

	private string DecryptText(string cipherText, string encryptionPrivateKey = "")
	{
		if (string.IsNullOrEmpty(cipherText))
		{
			return cipherText;
		}
		if (string.IsNullOrEmpty(encryptionPrivateKey))
		{
			encryptionPrivateKey = _securitySettings.EncryptionKey;
		}
		using TripleDES tripleDES = TripleDES.Create();
		tripleDES.Key = Encoding.ASCII.GetBytes(encryptionPrivateKey.Substring(0, 16));
		tripleDES.IV = Encoding.ASCII.GetBytes(encryptionPrivateKey.Substring(8, 8));
		byte[] data = Convert.FromBase64String(cipherText);
		return DecryptTextFromMemory(data, tripleDES.Key, tripleDES.IV);
	}

	private DecryptedLicense DecryptProductKey(string productKey, string encryptionKey)
	{
		try
		{
			Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(DecryptText(productKey, encryptionKey));
			if (dictionary == null)
			{
				return null;
			}
			dictionary.TryGetValue("ValidationDateUtc", out var value);
			if (!string.IsNullOrWhiteSpace(value))
			{
				DateTime dateTime = DateTime.ParseExact(value, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
				if (DateTime.UtcNow > dateTime)
				{
					return null;
				}
			}
			DecryptedLicense decryptedLicense = new DecryptedLicense();
			dictionary.TryGetValue("NOPVersion", out var value2);
			dictionary.TryGetValue("Domain", out var value3);
			decryptedLicense.NopVersion = ExtractVersionComponents(value2);
			decryptedLicense.Domain = value3;
			bool result = default(bool);
			decryptedLicense.IncludesSubdomains = (dictionary.TryGetValue("IncludesSubdomains", out var value4) && bool.TryParse(value4, out result)) & result;
			bool result2 = default(bool);
			decryptedLicense.SkipCheckDomain = (dictionary.TryGetValue("SkipCheckDomain", out var value5) && bool.TryParse(value5, out result2)) & result2;
			bool result3 = default(bool);
			decryptedLicense.SkipCheckFileName = (dictionary.TryGetValue("SkipCheckFileName", out var value6) && bool.TryParse(value6, out result3)) & result3;
			if (dictionary.TryGetValue("FileNames", out var value7))
			{
				decryptedLicense.FileNames = ExtractFileNames(value7);
			}
			return decryptedLicense;
		}
		catch (Exception ex)
		{
			_logger.InformationAsync("Failed to decrypt nop-station license product key: " + ex.Message, ex).Wait();
		}
		return null;
	}

	private IList<string> ExtractFileNames(string fileNames)
	{
		if (string.IsNullOrWhiteSpace(fileNames))
		{
			return new List<string>();
		}
		return fileNames.ToLower().Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
	}

	private int[] ExtractVersionComponents(string version)
	{
		if (version == null)
		{
			return null;
		}
		string[] array = version.Split('.');
		int num = 0;
		int num2 = 0;
		try
		{
			num = Convert.ToInt32(array[0]);
		}
		catch
		{
		}
		try
		{
			num2 = (int)((double)Convert.ToInt32(array[1]) / Math.Pow(10.0, array[1].Length - 1));
		}
		catch
		{
		}
		return new int[2] { num, num2 };
	}

	public async Task InsertLicenseAsync(License license)
	{
		await _licenseRepository.InsertAsync(license);
	}

	public async Task UpdateLicenseAsync(License license)
	{
		await _licenseRepository.UpdateAsync(license);
	}

	public async Task DeleteLicenseAsync(License license)
	{
		await _licenseRepository.DeleteAsync(license);
	}

	public async Task<IList<License>> GetLicensesAsync()
	{
		CacheKey key = _cacheManager.PrepareKey(CoreCacheDefaults.LicenseKey, _storeContext.GetCurrentStore());
		return await _cacheManager.GetAsync(key, () => _licenseRepository.Table.ToListAsync());
	}

	public KeyVerificationResult VerifyProductKey(string key, bool checkFileName = false, string fileName = "")
	{
		try
		{
			DecryptedLicense decryptedLicense = DecryptProductKey(key, Constants.LicenseKeySeed);
			if (decryptedLicense == null)
			{
				return KeyVerificationResult.InvalidProductKey;
			}
			if (decryptedLicense.NopVersion != null)
			{
				int[] array = ExtractVersionComponents("4.90");
				if (array[0] != decryptedLicense.NopVersion[0] || array[1] != decryptedLicense.NopVersion[1])
				{
					return KeyVerificationResult.InvalidForNOPVersion;
				}
			}
			if (!decryptedLicense.SkipCheckDomain)
			{
				string text = _httpContextAccessor.HttpContext.Request.Host.Host;
				if (text.StartsWith("www."))
				{
					string text2 = text;
					text = text2.Substring(4, text2.Length - 4);
				}
				if (decryptedLicense.Domain.StartsWith("www."))
				{
					string text2 = decryptedLicense.Domain;
					decryptedLicense.Domain = text2.Substring(4, text2.Length - 4);
				}
				if (text != decryptedLicense.Domain && (!decryptedLicense.IncludesSubdomains || (decryptedLicense.IncludesSubdomains && !text.EndsWith("." + decryptedLicense.Domain))))
				{
					return KeyVerificationResult.InvalidForDomain;
				}
			}
			if ((!decryptedLicense.SkipCheckFileName & checkFileName) && !decryptedLicense.FileNames.Contains(fileName))
			{
				return KeyVerificationResult.InvalidProduct;
			}
			return KeyVerificationResult.Valid;
		}
		catch (Exception ex)
		{
			_logger.ErrorAsync(ex.Message, ex).Wait();
			return KeyVerificationResult.InvalidProductKey;
		}
	}

	public async Task<bool> IsLicensedAsync(Assembly assembly)
	{
		if (_cachedLicensed.HasValue)
		{
			return _cachedLicensed.Value;
		}
		foreach (License item in await GetLicensesAsync())
		{
			if (VerifyProductKey(item.Key, checkFileName: true, assembly.GetName().Name.ToLower()) == KeyVerificationResult.Valid)
			{
				_cachedLicensed = true;
				break;
			}
		}
		return _cachedLicensed.HasValue && _cachedLicensed.Value;
	}
}
