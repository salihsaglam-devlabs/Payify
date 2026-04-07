using LinkPara.ApiGateway.Services.Card.HttpClients;
using LinkPara.ApiGateway.Services.Card.Models.CustomerWalletCard.Request;
using LinkPara.ApiGateway.Services.Card.Models.CustomerWalletCard.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Card;

public class CustomerWalletCardController : ApiControllerBase
{
    private readonly ICustomerWalletCardHttpClient _httpClient;
    public CustomerWalletCardController(ICustomerWalletCardHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    [Authorize(Policy = "CustomerWalletCard:ReadAll")]
    [HttpGet("")]
    public async Task<GetCustomerWalletCardsResponse> GetCustomerWalletCardsAsync([FromBody] GetCustomerWalletCardsRequest request)
    {
        return await _httpClient.GetCustomerWalletCardsAsync(request);
    }
}
