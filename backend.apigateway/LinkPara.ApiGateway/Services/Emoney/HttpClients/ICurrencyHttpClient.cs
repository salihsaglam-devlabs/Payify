using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface ICurrencyHttpClient
{
    Task<List<CurrencyDto>> GetAllAsync(CurrenciesFilterRequest request);
}

