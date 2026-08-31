namespace NopStation.Plugin.Misc.Core.Domains.SMS;

public static class SmsDefaults
{
	public static string MessageTemplatesAllCacheKey => "Nop.SmsTemplate.all-{0}";

	public static string MessageTemplatesByNameCacheKey => "Nop.SmsTemplate.name-{0}-{1}";

	public static string MessageTemplatesPrefixCacheKey => "Nop.SmsTemplate.";
}
