using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;

public record SmsTemplateModel : BaseNopEntityModel, ILocalizedModel<SmsTemplateLocalizedModel>, ILocalizedModel, IStoreMappingSupportedModel, IAclSupportedModel
{
	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.Fields.Name")]
	public string Name { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.Fields.Body")]
	public string Body { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.Fields.Active")]
	public bool Active { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.Fields.ProviderSystemName")]
	public string ProviderSystemName { get; set; }

	public IList<SelectListItem> AvailableSmsProviders { get; set; }

	public string AllowedTokens { get; set; }

	public IList<SmsTemplateLocalizedModel> Locales { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.Fields.LimitedToStores")]
	public IList<int> SelectedStoreIds { get; set; }

	public IList<SelectListItem> AvailableStores { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.Fields.LimitedToStores")]
	public string ListOfStores { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.Fields.AclCustomerRoles")]
	public IList<int> SelectedCustomerRoleIds { get; set; }

	public IList<SelectListItem> AvailableCustomerRoles { get; set; }

	public SmsTemplateModel()
	{
		Locales = new List<SmsTemplateLocalizedModel>();
		SelectedStoreIds = new List<int>();
		AvailableStores = new List<SelectListItem>();
		SelectedCustomerRoleIds = new List<int>();
		AvailableCustomerRoles = new List<SelectListItem>();
		AvailableSmsProviders = new List<SelectListItem>();
	}
}
