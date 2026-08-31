using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Domains;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Models;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Controllers;

[AuthorizeAdmin(false)]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public class NopStationLicenseController : BasePluginController
{
	private readonly ILicenseService _licenseService;

	private readonly INotificationService _notificationService;

	private readonly ILocalizationService _localizationService;

	public NopStationLicenseController(ILicenseService licenseService, INotificationService notificationService, ILocalizationService localizationService, IPermissionService permissionService)
	{
		_licenseService = licenseService;
		_notificationService = notificationService;
		_localizationService = localizationService;
	}

	[CheckPermission("ManageNopStationCoreLicense", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public IActionResult License()
	{
		return View(new LicenseModel());
	}

	[EditAccess(false)]
	[HttpPost]
	[CheckPermission("ManageNopStationCoreLicense", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public async Task<IActionResult> License(LicenseModel model)
	{
		switch (_licenseService.VerifyProductKey(model.LicenseString))
		{
		case KeyVerificationResult.InvalidProductKey:
		{
			INotificationService notificationService = _notificationService;
			notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidProductKey"));
			return View(model);
		}
		case KeyVerificationResult.InvalidProduct:
		{
			INotificationService notificationService = _notificationService;
			notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidProduct"));
			return View(model);
		}
		case KeyVerificationResult.InvalidForDomain:
		{
			INotificationService notificationService = _notificationService;
			notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidForDomain"));
			return View(model);
		}
		case KeyVerificationResult.InvalidForNOPVersion:
		{
			INotificationService notificationService = _notificationService;
			notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.InvalidForNOPVersion"));
			return View(model);
		}
		case KeyVerificationResult.Valid:
		{
			await _licenseService.InsertLicenseAsync(new License
			{
				Key = model.LicenseString
			});
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.License.Saved"));
			return RedirectToAction("License");
		}
		default:
			return RedirectToAction("License");
		}
	}
}
