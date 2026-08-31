using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Widgets.FirebasePushNotification.Models;

public record ConfigurationModel : BaseNopModel
{
	[NopResourceDisplayName("Plugins.Widgets.FirebasePushNotification.Fields.ApiKey")]
	public string ApiKey { get; set; } = string.Empty;

	[NopResourceDisplayName("Plugins.Widgets.FirebasePushNotification.Fields.AuthDomain")]
	public string AuthDomain { get; set; } = string.Empty;

	[NopResourceDisplayName("Plugins.Widgets.FirebasePushNotification.Fields.ProjectId")]
	public string ProjectId { get; set; } = string.Empty;

	[NopResourceDisplayName("Plugins.Widgets.FirebasePushNotification.Fields.MessagingSenderId")]
	public string MessagingSenderId { get; set; } = string.Empty;

	[NopResourceDisplayName("Plugins.Widgets.FirebasePushNotification.Fields.AppId")]
	public string AppId { get; set; } = string.Empty;

	[NopResourceDisplayName("Plugins.Widgets.FirebasePushNotification.Fields.VapidKey")]
	public string VapidKey { get; set; } = string.Empty;

	public IList<SelectListItem> AvailablePlatforms { get; set; } = new List<SelectListItem>();
}
