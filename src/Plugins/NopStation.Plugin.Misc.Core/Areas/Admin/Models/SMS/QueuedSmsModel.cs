using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;

public record QueuedSmsModel : BaseNopEntityModel
{
	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.Customer")]
	public int? CustomerId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.Customer")]
	public string CustomerName { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.Store")]
	public int StoreId { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.Store")]
	public string StoreName { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.PhoneNumber")]
	public string PhoneNumber { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.Body")]
	public string Body { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.SentTries")]
	public int SentTries { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.Error")]
	public string Error { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.CreatedOn")]
	public DateTime CreatedOn { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.SentOn")]
	public DateTime? SentOn { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.ProviderSystemName")]
	public string ProviderSystemName { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.Fields.ExternalMessageId")]
	public string ExternalMessageId { get; set; }
}
