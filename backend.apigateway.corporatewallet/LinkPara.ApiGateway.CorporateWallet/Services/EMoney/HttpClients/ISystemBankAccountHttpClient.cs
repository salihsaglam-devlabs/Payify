using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients
{
    public interface ISystemBankAccountHttpClient
    {
        Task<List<SystemBankAccountDto>> GetSystemBankAccountsAsync();
    }
}