using LinkPara.ApiGateway.Card.Services.Card.Models;

namespace LinkPara.ApiGateway.Card.Services.Card.HttpClients;
public interface IDebitAuthorizationHttpClient
{
    Task<DebitAuthorizationResponse> ProcessDebitAuthorizationAsync(DebitAuthorizationRequest request);
}
