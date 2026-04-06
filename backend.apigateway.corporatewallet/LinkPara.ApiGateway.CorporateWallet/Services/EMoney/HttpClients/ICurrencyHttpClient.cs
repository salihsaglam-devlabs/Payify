using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public interface ICurrencyHttpClient
{
    Task<List<CurrencyDto>> GetAllAsync(CurrenciesFilterRequest request);
}
