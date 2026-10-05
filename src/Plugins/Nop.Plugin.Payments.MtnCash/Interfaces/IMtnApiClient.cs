namespace Nop.Plugin.Payments.MtnCash.Interfaces;
public interface IMtnApiClient
{
    Task<string> PostAsync(string requestName, object body);
    Task<string> GetAsync(string requestName, object query);
}

