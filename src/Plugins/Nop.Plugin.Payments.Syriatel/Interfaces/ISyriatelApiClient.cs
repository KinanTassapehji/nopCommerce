namespace Nop.Plugin.Payments.Syriatel.Interfaces;

public interface ISyriatelApiClient
{
    Task<string> PostAsync(string endpoint, object body);
}
