using LinkPara.ApiGateway.Merchant.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Emoney.HttpClients;

public interface ICurrencyHttpClient
{
    Task<List<CurrencyDto>> GetAllAsync(CurrenciesFilterRequest request);
}
