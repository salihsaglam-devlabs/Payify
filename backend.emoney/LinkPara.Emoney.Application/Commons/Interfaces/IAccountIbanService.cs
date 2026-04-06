namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IAccountIbanService
{
    public Task<bool> ValidateIbanAsync(string identityNo, string iban);
}