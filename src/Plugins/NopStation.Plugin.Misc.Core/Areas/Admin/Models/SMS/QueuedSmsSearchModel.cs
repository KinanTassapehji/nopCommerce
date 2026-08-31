using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.Core.Areas.Admin.Models.SMS;

public record QueuedSmsSearchModel : BaseSearchModel
{
	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.List.SearchStartDate")]
	[UIHint("DateNullable")]
	public DateTime? SearchStartDate { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.List.SearchEndDate")]
	[UIHint("DateNullable")]
	public DateTime? SearchEndDate { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.List.SearchPhoneNumber")]
	public string SearchPhoneNumber { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.List.SearchLoadNotSent")]
	public bool SearchLoadNotSent { get; set; }

	[NopResourceDisplayName("Admin.NopStation.Core.QueuedSms.List.SearchMaxSentTries")]
	public int SearchMaxSentTries { get; set; }
}
