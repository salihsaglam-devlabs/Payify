using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients
{
    public interface IBankHttpClient
    {
        Task<List<BankDto>> GetBanksAsync(string iban = null);
    }
}
