namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IIbanBlacklistService
{
    Task<bool> IsBlacklistedAsync(string iban);
}