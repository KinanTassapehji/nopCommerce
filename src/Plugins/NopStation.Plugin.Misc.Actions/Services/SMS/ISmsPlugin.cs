using System.Threading.Tasks;
using Nop.Services.Common;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Domains.SMS;

namespace NopStation.Plugin.Misc.Core.Services.SMS;

public interface ISmsPlugin : IMiscPlugin, IPlugin
{
	Task<bool> IsActiveAsync();

	Task<SmsSendResult> SendSmsAsync(string phoneNumber, string messageBody);

	Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
}
