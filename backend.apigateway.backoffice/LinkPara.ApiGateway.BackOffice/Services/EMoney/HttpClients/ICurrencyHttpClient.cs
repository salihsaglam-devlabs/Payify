using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients
{
    public interface ICurrencyHttpClient
    {
        Task<List<CurrencyDto>> GetAllAsync(CurrenciesFilterRequest request);
    }
}
