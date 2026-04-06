using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients
{
    public interface IBankHttpClient
    {
        Task<List<BankDto>> GetBanksAsync(string iban = null);
    }
}
