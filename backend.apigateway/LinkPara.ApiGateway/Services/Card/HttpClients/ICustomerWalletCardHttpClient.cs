using LinkPara.ApiGateway.Services.Card.Models.CustomerWalletCard.Request;
using LinkPara.ApiGateway.Services.Card.Models.CustomerWalletCard.Response;

namespace LinkPara.ApiGateway.Services.Card.HttpClients;

public interface ICustomerWalletCardHttpClient
{
    Task<GetCustomerWalletCardsResponse> GetCustomerWalletCardsAsync(GetCustomerWalletCardsRequest request);
}