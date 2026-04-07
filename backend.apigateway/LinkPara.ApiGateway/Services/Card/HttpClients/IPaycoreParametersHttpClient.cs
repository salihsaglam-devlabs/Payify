using LinkPara.ApiGateway.Services.Card.Models.PaycoreParameters.Response;

namespace LinkPara.ApiGateway.Services.Card.HttpClients;

public interface IPaycoreParametersHttpClient
{
    Task<GetProductsResponse> GetProductsAsync();
}