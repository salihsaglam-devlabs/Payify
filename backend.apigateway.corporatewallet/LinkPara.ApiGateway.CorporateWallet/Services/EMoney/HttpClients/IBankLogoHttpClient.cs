using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients
{
    public interface IBankLogoHttpClient
    {
        Task<BankLogoDto> GetBankLogoAsync(Guid bankId);
    }
}