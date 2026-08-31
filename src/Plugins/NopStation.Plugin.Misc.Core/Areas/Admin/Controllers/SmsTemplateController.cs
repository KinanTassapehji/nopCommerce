using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Areas.Admin.Factories;
using NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Services.SMS;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Controllers;

public class SmsTemplateController : NopStationAdminController
{
	private readonly ISmsTemplateService _smsTemplateService;

	private readonly ISmsTemplateModelFactory _smsTemplateModelFactory;

	private readonly ILocalizationService _localizationService;

	private readonly ILocalizedEntityService _localizedEntityService;

	private readonly INotificationService _notificationService;

	private readonly IStoreMappingService _storeMappingService;

	private readonly IStoreService _storeService;

	private readonly IAclService _aclService;

	private readonly ICustomerService _customerService;

	public SmsTemplateController(ISmsTemplateService smsTemplateService, ISmsTemplateModelFactory smsTemplateModelFactory, ILocalizationService localizationService, ILocalizedEntityService localizedEntityService, INotificationService notificationService, IStoreMappingService storeMappingService, IStoreService storeService, IAclService aclService, ICustomerService customerService)
	{
		_smsTemplateService = smsTemplateService;
		_smsTemplateModelFactory = smsTemplateModelFactory;
		_localizationService = localizationService;
		_localizedEntityService = localizedEntityService;
		_notificationService = notificationService;
		_storeMappingService = storeMappingService;
		_storeService = storeService;
		_aclService = aclService;
		_customerService = customerService;
	}

	protected virtual async Task UpdateLocalesAsync(SmsTemplate smsTemplate, SmsTemplateModel model)
	{
		foreach (SmsTemplateLocalizedModel locale in model.Locales)
		{
			await _localizedEntityService.SaveLocalizedValueAsync(smsTemplate, (SmsTemplate x) => x.Body, locale.Body, locale.LanguageId);
		}
	}

	protected virtual async Task SaveStoreMappingsAsync(SmsTemplate smsTemplate, SmsTemplateModel model)
	{
		smsTemplate.LimitedToStores = model.SelectedStoreIds.Any();
		await _smsTemplateService.UpdateSmsTemplateAsync(smsTemplate);
		IList<StoreMapping> existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(smsTemplate);
		foreach (Store store in await _storeService.GetAllStoresAsync())
		{
			if (model.SelectedStoreIds.Contains(store.Id))
			{
				if (!existingStoreMappings.Any((StoreMapping sm) => sm.StoreId == store.Id))
				{
					await _storeMappingService.InsertStoreMappingAsync(smsTemplate, store.Id);
				}
				continue;
			}
			StoreMapping storeMapping = existingStoreMappings.FirstOrDefault((StoreMapping sm) => sm.StoreId == store.Id);
			if (storeMapping != null)
			{
				await _storeMappingService.DeleteStoreMappingAsync(storeMapping);
			}
		}
	}

	protected virtual async Task SaveAclMappingsAsync(SmsTemplate smsTemplate, SmsTemplateModel model)
	{
		smsTemplate.SubjectToAcl = model.SelectedCustomerRoleIds.Any();
		await _smsTemplateService.UpdateSmsTemplateAsync(smsTemplate);
		IList<AclRecord> existingAclRecords = await _aclService.GetAclRecordsAsync(smsTemplate);
		foreach (CustomerRole customerRole in await _customerService.GetAllCustomerRolesAsync(showHidden: true))
		{
			if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
			{
				if (!existingAclRecords.Any((AclRecord acl) => acl.CustomerRoleId == customerRole.Id))
				{
					await _aclService.InsertAclRecordAsync(smsTemplate, customerRole.Id);
				}
				continue;
			}
			AclRecord aclRecord = existingAclRecords.FirstOrDefault((AclRecord acl) => acl.CustomerRoleId == customerRole.Id);
			if (aclRecord != null)
			{
				await _aclService.DeleteAclRecordAsync(aclRecord);
			}
		}
	}

	[CheckPermission("ManageNopStationSmsTemplates", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual Task<IActionResult> Index()
	{
		return Task.FromResult((IActionResult)RedirectToAction("List"));
	}

	[CheckPermission("ManageNopStationSmsTemplates", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual Task<IActionResult> List()
	{
		SmsTemplateSearchModel model = _smsTemplateModelFactory.PrepareSmsTemplateSearchModel(new SmsTemplateSearchModel());
		return Task.FromResult((IActionResult)View(model));
	}

	[CheckPermission("ManageNopStationSmsTemplates", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	[HttpPost]
	public virtual async Task<IActionResult> List(SmsTemplateSearchModel searchModel)
	{
		return Json(await _smsTemplateModelFactory.PrepareSmsTemplateListModelAsync(searchModel));
	}

	[CheckPermission("ManageNopStationSmsTemplates", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> Edit(int id)
	{
		SmsTemplate smsTemplate = await _smsTemplateService.GetSmsTemplateByIdAsync(id);
		if (smsTemplate == null)
		{
			return RedirectToAction("List");
		}
		return View(await _smsTemplateModelFactory.PrepareSmsTemplateModelAsync(null, smsTemplate));
	}

	[CheckPermission("ManageNopStationSmsTemplates", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	[HttpPost]
	[ParameterBasedOnFormName("save-continue", "continueEditing")]
	public virtual async Task<IActionResult> Edit(SmsTemplateModel model, bool continueEditing)
	{
		SmsTemplate smsTemplate = await _smsTemplateService.GetSmsTemplateByIdAsync(model.Id);
		if (smsTemplate == null)
		{
			return RedirectToAction("List");
		}
		if (base.ModelState.IsValid)
		{
			smsTemplate = model.ToEntity(smsTemplate);
			await _smsTemplateService.UpdateSmsTemplateAsync(smsTemplate);
			await UpdateLocalesAsync(smsTemplate, model);
			await SaveStoreMappingsAsync(smsTemplate, model);
			await SaveAclMappingsAsync(smsTemplate, model);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.SmsTemplates.Updated"));
			if (!continueEditing)
			{
				return RedirectToAction("List");
			}
			return RedirectToAction("Edit", new
			{
				id = smsTemplate.Id
			});
		}
		model = await _smsTemplateModelFactory.PrepareSmsTemplateModelAsync(model, smsTemplate, excludeProperties: true);
		return View(model);
	}

	[HttpPost]
	[ActionName("Edit")]
	[FormValueRequired(new string[] { "sms-template-copy" })]
	[CheckPermission("ManageNopStationSmsTemplates", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	public virtual async Task<IActionResult> CopyTemplate(SmsTemplateModel model)
	{
		SmsTemplate smsTemplate = await _smsTemplateService.GetSmsTemplateByIdAsync(model.Id);
		if (smsTemplate == null)
		{
			return RedirectToAction("List");
		}
		try
		{
			SmsTemplate newSmsTemplate = await _smsTemplateService.CopySmsTemplateAsync(smsTemplate);
			INotificationService notificationService = _notificationService;
			notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.SmsTemplates.Copied"));
			return RedirectToAction("Edit", new
			{
				id = newSmsTemplate.Id
			});
		}
		catch (Exception ex)
		{
			_notificationService.ErrorNotification(ex.Message);
			return RedirectToAction("Edit", new
			{
				id = model.Id
			});
		}
	}

	[CheckPermission("ManageNopStationSmsTemplates", CheckPermissionAttribute.CheckPermissionResultType.Default)]
	[HttpPost]
	public virtual async Task<IActionResult> Delete(int id)
	{
		SmsTemplate smsTemplate = await _smsTemplateService.GetSmsTemplateByIdAsync(id);
		if (smsTemplate == null)
		{
			return RedirectToAction("List");
		}
		await _smsTemplateService.DeleteSmsTemplateAsync(smsTemplate);
		INotificationService notificationService = _notificationService;
		notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Core.SmsTemplates.Deleted"));
		return RedirectToAction("List");
	}
}
