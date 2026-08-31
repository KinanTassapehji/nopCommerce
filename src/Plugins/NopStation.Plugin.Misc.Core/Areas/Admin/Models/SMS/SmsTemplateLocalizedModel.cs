using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;

public class SmsTemplateLocalizedModel : ILocalizedLocaleModel
{
	public int LanguageId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.SmsTemplates.Fields.Body")]
	public string Body { get; set; }
}
