namespace Nop.Plugin.Payments.Syriatel.Interfaces;
public interface ISyriatelTokenProvider
{
    Task<string> GetTokenAsync();
}
internal sealed class SyriatelTokenState
{
    public string Token { get; set; } = string.Empty;
    public DateTime GeneratedAtUtc { get; set; }
}
