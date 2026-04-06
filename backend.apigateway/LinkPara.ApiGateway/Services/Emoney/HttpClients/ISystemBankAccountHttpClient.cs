using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients
{
    public interface ISystemBankAccountHttpClient
    {
        Task<List<SystemBankAccountDto>> GetSystemBankAccountsAsync();
    }
}