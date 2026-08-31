using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Misc.Core.Domains.SMS;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.Core;

public class NopStationCorePlugin : BasePlugin, INopStationPlugin, IPlugin
{
	private readonly IWebHelper _webHelper;

	private readonly ISettingService _settingService;

	private readonly IScheduleTaskService _scheduleTaskService;

	public NopStationCorePlugin(IWebHelper webHelper, ISettingService settingService, IScheduleTaskService scheduleTaskService)
	{
		_webHelper = webHelper;
		_settingService = settingService;
		_scheduleTaskService = scheduleTaskService;
	}

	public override string GetConfigurationPageUrl()
	{
		return _webHelper.GetStoreLocation() + "Admin/NopStationCore/Configure";
	}

	public override async Task InstallAsync()
	{
		NopStationCoreSettings settings = new NopStationCoreSettings
		{
			AllowedCustomerRoleIds = new List<int> { 1, 2 }
		};
		await _settingService.SaveSettingAsync(settings);
		await _settingService.SaveSettingAsync(new SmsSettings
		{
			MaxSendTries = 3,
			MaxMessagesPerBatch = 100,
			DelayBetweenMessagesMs = 100,
			EnableDetailedLogging = false
		});
		if (await _scheduleTaskService.GetTaskByTypeAsync(typeof(QueuedSmsSendTask).FullName) == null)
		{
			await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
			{
				Name = "Send Queued SMS (All Providers)",
				Seconds = 60,
				Type = typeof(QueuedSmsSendTask).FullName,
				Enabled = true,
				StopOnError = false
			});
		}
		await this.InstallPluginAsync();
		await base.InstallAsync();
	}

	public override async Task UninstallAsync()
	{
		ScheduleTask scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(typeof(QueuedSmsSendTask).FullName);
		if (scheduleTask != null)
		{
			await _scheduleTaskService.DeleteTaskAsync(scheduleTask);
		}
		await _settingService.DeleteSettingAsync<SmsSettings>();
		await this.UninstallPluginAsync();
		await base.UninstallAsync();
	}

	public IDictionary<string, string> GetPluginResources()
	{
		return new Dictionary<string, string>
		{
			["Admin.NopStation.Core.AssemblyInfo"] = "Nop-Station assembly information",
			["Admin.NopStation.Core.Configuration"] = "Core settings",
			["Admin.NopStation.Core.LocaleResources"] = "String resources",
			["Admin.NopStation.Core.ACL"] = "Access control list",
			["Admin.NopStation.Core.License"] = "License",
			["Admin.NopStation.Core.Menu.NopStation"] = "Nop Station",
			["Admin.NopStation.Core.Menu.AssemblyInfo"] = "Assembly information",
			["Admin.NopStation.Core.Menu.Configuration"] = "Configuration",
			["Admin.NopStation.Core.Menu.LocaleResources"] = "String resources",
			["Admin.NopStation.Core.Menu.ACL"] = "Access control list",
			["Admin.NopStation.Core.Menu.License"] = "License",
			["Admin.NopStation.Core.Menu.Core"] = "Core settings",
			["Admin.NopStation.Core.Menu.Themes"] = "Themes",
			["Admin.NopStation.Core.Menu.Plugins"] = "Plugins",
			["Admin.NopStation.Core.Menu.ReportBug"] = "Report a bug",
			["Admin.NopStation.Core.License.InvalidProductKey"] = "Your product key is not valid.",
			["Admin.NopStation.Core.License.InvalidForDomain"] = "Your product key is not valid for this domain.",
			["Admin.NopStation.Core.License.InvalidForNOPVersion"] = "Your product key is not valid for this nopCommerce version.",
			["Admin.NopStation.Core.License.Saved"] = "Your product key has been saved successfully.",
			["Admin.NopStation.Core.License.LicenseString"] = "License string",
			["Admin.NopStation.Core.License.LicenseString.Hint"] = "Nop-station plugin/theme license string.",
			["Admin.NopStation.Common.Menu.Documentation"] = "Documentation",
			["Admin.NopStation.Core.Resources.EditAccessDenied"] = "For security purposes, the feature you have requested is not available on this site.",
			["Admin.NopStation.Core.Resources.FailedToSave"] = "Failed to save resource string.",
			["Admin.NopStation.Core.Resources.Fields.Name"] = "Name",
			["Admin.NopStation.Core.Resources.Fields.Value"] = "Value",
			["Admin.NopStation.Core.Resources.List.SearchPluginSystemName"] = "Plugin",
			["Admin.NopStation.Core.Resources.List.SearchPluginSystemName.Hint"] = "Search resource string by plugin.",
			["Admin.NopStation.Core.Resources.List.SearchResourceName"] = "Resource name",
			["Admin.NopStation.Core.Resources.List.SearchResourceName.Hint"] = "Search resource string by resource name.",
			["Admin.NopStation.Core.Resources.List.SearchLanguageId"] = "Language",
			["Admin.NopStation.Core.Resources.List.SearchLanguageId.Hint"] = "Search resource string by language.",
			["Admin.NopStation.Core.Resources.List.SearchPluginSystemName.All"] = "All",
			["Admin.NopStation.Core.Configuration.Fields.EnableCORS.ChangeHint"] = "Restart your application after changing this setting value.",
			["Admin.NopStation.Core.Configuration.Fields.EnableCORS"] = "Enable CORS",
			["Admin.NopStation.Core.Configuration.Fields.EnableCORS.Hint"] = "Check to enable CORS. It will add \"Access-Control-Allow-Origin\" header for every api response.",
			["Admin.NopStation.Core.Configuration.AdminCanNotBeRestricted"] = "Admin role can not be restricted.",
			["Admin.NopStation.Core.Configuration.Fields.RestrictMainMenuByCustomerRoles"] = "Restrict main menu by customer roles",
			["Admin.NopStation.Core.Configuration.Fields.RestrictMainMenuByCustomerRoles.Hint"] = "Restrict main menu (Nop Station) by customer roles.",
			["Admin.NopStation.Core.Configuration.Fields.AllowedCustomerRoles"] = "Allowed customer roles",
			["Admin.NopStation.Core.Configuration.Fields.AllowedCustomerRoles.Hint"] = "Select allowed customer roles to access Nop Station plugin menus. Make sure proper access provided for these customer roles from 'Access control list' page.",
			["Admin.NopStation.Core.Menu.Sms"] = "SMS",
			["Admin.NopStation.Core.Menu.SmsSettings"] = "Settings",
			["Admin.NopStation.Core.Menu.SmsProviders"] = "Providers",
			["Admin.NopStation.Core.Menu.SmsTemplates"] = "SMS templates",
			["Admin.NopStation.Core.Menu.QueuedSms"] = "SMS queue",
			["Admin.NopStation.Core.SmsSettings"] = "SMS settings",
			["Admin.NopStation.Core.SmsSettings.Fields.MaxSendTries"] = "Max send tries",
			["Admin.NopStation.Core.SmsSettings.Fields.MaxSendTries.Hint"] = "Maximum number of send attempts before a queued SMS is considered failed.",
			["Admin.NopStation.Core.SmsSettings.Fields.MaxMessagesPerBatch"] = "Max messages per batch",
			["Admin.NopStation.Core.SmsSettings.Fields.MaxMessagesPerBatch.Hint"] = "Maximum number of queued SMSs processed in a single batch.",
			["Admin.NopStation.Core.SmsSettings.Fields.DelayBetweenMessagesMs"] = "Delay between messages (ms)",
			["Admin.NopStation.Core.SmsSettings.Fields.DelayBetweenMessagesMs.Hint"] = "Delay in milliseconds between sending individual SMSs.",
			["Admin.NopStation.Core.SmsSettings.Fields.EnableDetailedLogging"] = "Enable detailed logging",
			["Admin.NopStation.Core.SmsSettings.Fields.EnableDetailedLogging.Hint"] = "Check to enable detailed logging for SMS send operations.",
			["Admin.NopStation.Core.SmsProviders"] = "SMS providers",
			["Admin.NopStation.Core.SmsProviders.Fields.FriendlyName"] = "Friendly name",
			["Admin.NopStation.Core.SmsProviders.Fields.SystemName"] = "System name",
			["Admin.NopStation.Core.SmsProviders.Fields.DisplayOrder"] = "Display order",
			["Admin.NopStation.Core.SmsProviders.Fields.IsActive"] = "Is active",
			["Admin.NopStation.Core.SmsProviders.Configure"] = "Configure",
			["Admin.NopStation.Core.SmsTemplates.List"] = "SMS templates",
			["Admin.NopStation.Core.SmsTemplates.AddNew"] = "Add new template",
			["Admin.NopStation.Core.SmsTemplates.EditDetails"] = "Edit template",
			["Admin.NopStation.Core.SmsTemplates.BackToList"] = "back to template list",
			["Admin.NopStation.Core.SmsTemplates.Created"] = "SMS template created successfully.",
			["Admin.NopStation.Core.SmsTemplates.Updated"] = "SMS template updated successfully.",
			["Admin.NopStation.Core.SmsTemplates.Deleted"] = "SMS template deleted successfully.",
			["Admin.NopStation.Core.SmsTemplates.Copied"] = "SMS template has been copied successfully.",
			["Admin.NopStation.Core.SmsTemplates.Copy"] = "Copy template",
			["Admin.NopStation.Core.SmsTemplates.List.SearchKeywords"] = "Search keywords",
			["Admin.NopStation.Core.SmsTemplates.List.SearchKeywords.Hint"] = "Keywords to search by name or body.",
			["Admin.NopStation.Core.SmsTemplates.List.SearchActiveId"] = "Is active",
			["Admin.NopStation.Core.SmsTemplates.List.SearchActiveId.Hint"] = "Search by \"Active\" property.",
			["Admin.NopStation.Core.SmsTemplates.Fields.AllowedTokens"] = "Allowed SMS tokens",
			["Admin.NopStation.Core.SmsTemplates.Fields.AllowedTokens.Hint"] = "This is a list of the message tokens you can use in your SMSs.",
			["Admin.NopStation.Core.SmsTemplates.Fields.Name"] = "Name",
			["Admin.NopStation.Core.SmsTemplates.Fields.Name.Hint"] = "The name of this template (read only).",
			["Admin.NopStation.Core.SmsTemplates.Fields.Body"] = "Body",
			["Admin.NopStation.Core.SmsTemplates.Fields.Body.Hint"] = "The body of your SMS.",
			["Admin.NopStation.Core.SmsTemplates.Fields.Active"] = "Active",
			["Admin.NopStation.Core.SmsTemplates.Fields.Active.Hint"] = "Indicating whether the SMS template is active.",
			["Admin.NopStation.Core.SmsTemplates.Fields.ProviderSystemName"] = "SMS provider",
			["Admin.NopStation.Core.SmsTemplates.Fields.ProviderSystemName.Hint"] = "Select the SMS provider plugin to use for sending SMSs with this template. If not selected, the default provider will be used.",
			["Admin.NopStation.Core.SmsTemplates.Fields.AclCustomerRoles"] = "Limited to customer roles",
			["Admin.NopStation.Core.SmsTemplates.Fields.AclCustomerRoles.Hint"] = "Choose one or several customer roles i.e. administrators, vendors, guests, who will be able to use or see this item. If you don't need this option just leave this field empty.",
			["Admin.NopStation.Core.SmsTemplates.Fields.LimitedToStores"] = "Limited to stores",
			["Admin.NopStation.Core.SmsTemplates.Fields.LimitedToStores.Hint"] = "Option to limit this template to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty.",
			["Admin.NopStation.Core.SmsTemplates.Fields.Body.Required"] = "The SMS template body is required.",
			["Admin.NopStation.Core.QueuedSms.List"] = "SMS queue",
			["Admin.NopStation.Core.QueuedSms.BackToList"] = "back to queue list",
			["Admin.NopStation.Core.QueuedSms.ViewDetails"] = "SMS details",
			["Admin.NopStation.Core.QueuedSms.Deleted"] = "Queued SMS deleted successfully.",
			["Admin.NopStation.Core.QueuedSms.DeletedSelected"] = "Queued SMS deleted successfully.",
			["Admin.NopStation.Core.QueuedSms.DeletedAll"] = "All queued SMSs deleted successfully.",
			["Admin.NopStation.Core.QueuedSms.Resent"] = "SMS resent successfully.",
			["Admin.NopStation.Core.QueuedSms.Resend"] = "Resend",
			["Admin.NopStation.Core.QueuedSms.ResendFailed"] = "Failed to resend SMS",
			["Admin.NopStation.Core.QueuedSms.RequeueSelected"] = "Requeue selected",
			["Admin.NopStation.Core.QueuedSms.DeleteSelected"] = "Delete selected",
			["Admin.NopStation.Core.QueuedSms.DeleteAll"] = "Delete all",
			["Admin.NopStation.Core.QueuedSms.List.SearchStartDate"] = "Start date",
			["Admin.NopStation.Core.QueuedSms.List.SearchStartDate.Hint"] = "The start date for the search.",
			["Admin.NopStation.Core.QueuedSms.List.SearchEndDate"] = "End date",
			["Admin.NopStation.Core.QueuedSms.List.SearchEndDate.Hint"] = "The end date for the search.",
			["Admin.NopStation.Core.QueuedSms.List.SearchPhoneNumber"] = "Phone number",
			["Admin.NopStation.Core.QueuedSms.List.SearchPhoneNumber.Hint"] = "Phone number.",
			["Admin.NopStation.Core.QueuedSms.List.SearchLoadNotSent"] = "Load not sent only",
			["Admin.NopStation.Core.QueuedSms.List.SearchLoadNotSent.Hint"] = "Only load SMSs into queue that have not been sent yet.",
			["Admin.NopStation.Core.QueuedSms.List.SearchMaxSentTries"] = "Max sent tries",
			["Admin.NopStation.Core.QueuedSms.List.SearchMaxSentTries.Hint"] = "The maximum number of attempts to send a message.",
			["Admin.NopStation.Core.QueuedSms.All"] = "All",
			["Admin.NopStation.Core.QueuedSms.Unknown"] = "Unknown",
			["Admin.NopStation.Core.QueuedSms.Fields.Customer"] = "Customer",
			["Admin.NopStation.Core.QueuedSms.Fields.Customer.Hint"] = "The customer.",
			["Admin.NopStation.Core.QueuedSms.Fields.Store"] = "Store",
			["Admin.NopStation.Core.QueuedSms.Fields.Store.Hint"] = "The store.",
			["Admin.NopStation.Core.QueuedSms.Fields.PhoneNumber"] = "Phone number",
			["Admin.NopStation.Core.QueuedSms.Fields.PhoneNumber.Hint"] = "Phone number.",
			["Admin.NopStation.Core.QueuedSms.Fields.Body"] = "Body",
			["Admin.NopStation.Core.QueuedSms.Fields.Body.Hint"] = "Message body.",
			["Admin.NopStation.Core.QueuedSms.Fields.SentTries"] = "Sent tries",
			["Admin.NopStation.Core.QueuedSms.Fields.SentTries.Hint"] = "The number of times to attempt to send this message.",
			["Admin.NopStation.Core.QueuedSms.Fields.Error"] = "Error",
			["Admin.NopStation.Core.QueuedSms.Fields.Error.Hint"] = "The error message.",
			["Admin.NopStation.Core.QueuedSms.Fields.CreatedOn"] = "Created on",
			["Admin.NopStation.Core.QueuedSms.Fields.CreatedOn.Hint"] = "Date/Time message added to queue.",
			["Admin.NopStation.Core.QueuedSms.Fields.SentOn"] = "Sent on",
			["Admin.NopStation.Core.QueuedSms.Fields.SentOn.Hint"] = "The date/time message was sent.",
			["Admin.NopStation.Core.QueuedSms.Fields.ProviderSystemName"] = "Provider",
			["Admin.NopStation.Core.QueuedSms.Fields.ProviderSystemName.Hint"] = "The SMS provider system name.",
			["Admin.NopStation.Core.QueuedSms.Fields.ExternalMessageId"] = "External message ID",
			["Admin.NopStation.Core.QueuedSms.Fields.ExternalMessageId.Hint"] = "The external message ID from the SMS provider.",
			["NopStation.Core.Request.Common.Ok"] = "Request success",
			["NopStation.Core.Request.Common.BadRequest"] = "Bad request",
			["NopStation.Core.Request.Common.Unauthorized"] = "Unauthorized",
			["NopStation.Core.Request.Common.NotFound"] = "Not found",
			["NopStation.Core.Request.Common.InternalServerError"] = "Internal server error",
			["Admin.NopStation.Core.Menu.Marketplace"] = "Marketplace",
			["Admin.NopStation.Core.Marketplace"] = "NopStation Marketplace",
			["Admin.NopStation.Core.Marketplace.NoPluginsFound"] = "No products were found. The marketplace may be temporarily unavailable or there was an error fetching the list.",
			["Admin.NopStation.Core.Marketplace.ShowingProducts"] = "Showing {0} of {1} products.",
			["Admin.NopStation.Core.Marketplace.Free"] = "Free",
			["Admin.NopStation.Core.Marketplace.Fields.Name"] = "Name",
			["Admin.NopStation.Core.Marketplace.Fields.ShortDescription"] = "Description",
			["Admin.NopStation.Core.Marketplace.Fields.Version"] = "Version",
			["Admin.NopStation.Core.Marketplace.Fields.SupportedVersions"] = "Supported nopCommerce versions",
			["Admin.NopStation.Core.Marketplace.Fields.PictureUrl"] = "Product image",
			["Admin.NopStation.Core.Marketplace.Fields.Price"] = "Price",
			["Admin.NopStation.Core.Marketplace.Fields.FormattedPrice"] = "Price",
			["Admin.NopStation.Core.Marketplace.Fields.OldPrice"] = "Old price",
			["Admin.NopStation.Core.Marketplace.Fields.FormattedOldPrice"] = "Old price",
			["Admin.NopStation.Core.Marketplace.Category.Fields.Name"] = "Category name",
			["Admin.NopStation.Core.Marketplace.Search.SearchText"] = "Search",
			["Admin.NopStation.Core.Marketplace.Search.SearchText.Placeholder"] = "Search products...",
			["Admin.NopStation.Core.Marketplace.Search.Category"] = "Category",
			["Admin.NopStation.Core.Marketplace.Search.Category.All"] = "All categories",
			["Admin.NopStation.Core.Marketplace.Search.PaidFilter"] = "Pricing",
			["Admin.NopStation.Core.Marketplace.Search.PaidFilter.All"] = "All",
			["Admin.NopStation.Core.Marketplace.Search.PaidFilter.Paid"] = "Paid only",
			["Admin.NopStation.Core.Marketplace.Search.PaidFilter.Free"] = "Free only",
			["Admin.NopStation.Core.Marketplace.Search.VersionFilter"] = "Version",
			["Admin.NopStation.Core.Marketplace.Search.VersionFilter.All"] = "All versions",
			["Admin.NopStation.Core.Marketplace.Search.VersionFilter.Current"] = "Current version only",
			["Admin.NopStation.Core.Marketplace.Button.Install"] = "Install",
			["Admin.NopStation.Core.Marketplace.Button.Installed"] = "Installed",
			["Admin.NopStation.Core.Marketplace.Button.Details"] = "Details",
			["Admin.NopStation.Core.Marketplace.Button.BuyNow"] = "Buy Now",
			["Admin.NopStation.Core.Marketplace.Button.NeedUpgrade"] = "Need Upgrade?",
			["Admin.NopStation.Core.Marketplace.Install.RestartRequired"] = "Plugin downloaded successfully. Please restart the application to complete the installation.",
			["Admin.NopStation.Core.Marketplace.Install.AlreadyInstalled"] = "This plugin is already installed.",
			["Admin.NopStation.Core.Marketplace.Install.InvalidDownloadUrl"] = "The plugin download URL is not valid.",
			["Admin.NopStation.Core.Marketplace.Install.UploadFailed"] = "Failed to upload the plugin package. Please try again.",
			["Admin.NopStation.Core.Marketplace.Install.Failed"] = "An error occurred while installing the plugin: {0}",
			["Admin.NopStation.Core.Marketplace.Install.Downloading"] = "Downloading and installing...",
			["Admin.NopStation.Core.Marketplace.Install.InstalledRestart"] = "Installed (restart required)",
			["Admin.NopStation.Core.Marketplace.Install.Title"] = "Install Plugin",
			["Admin.NopStation.Core.Marketplace.Description.ShowMore"] = "Show more",
			["Admin.NopStation.Core.Marketplace.Description.ShowLess"] = "Show less",
			["Admin.NopStation.Core.Marketplace.Image.FullPreview"] = "Full image preview",
			["Admin.NopStation.Core.Marketplace.Category.ShowMore"] = "Show more",
			["Admin.NopStation.Core.Marketplace.Category.ShowLess"] = "Show less",
			["Admin.NopStation.Core.Marketplace.UpgradeRequest.Title"] = "Request for Upgrade",
			["Admin.NopStation.Core.Marketplace.UpgradeRequest.Confirm"] = "Confirm",
			["Admin.NopStation.Core.Marketplace.UpgradeRequest.Cancel"] = "Cancel",
			["Admin.NopStation.Core.Marketplace.UpgradeRequest.Close"] = "Close",
			["Admin.NopStation.Core.Marketplace.UpgradeRequest.Sending"] = "Sending...",
			["Admin.NopStation.Core.Marketplace.UpgradeRequest.Sent"] = "Your upgrade request has been submitted successfully.",
			["Admin.NopStation.Core.Marketplace.UpgradeRequest.Failed"] = "An error occurred while submitting the upgrade request: {0}",
			["Admin.NopStation.Core.Marketplace.UpgradeRequest.Failed.Generic"] = "An error occurred. Please try again.",
			["Admin.NopStation.Core.Marketplace.UpgradeRequest.InvalidSystemName"] = "The plugin system name is not valid.",
			["Admin.NopStation.Core.Marketplace.StoreUrlEmailShareWarning"] = "Your store URL and current user's email will be shared with nopStation to process this request. Do you want to continue?"
		};
	}
}
