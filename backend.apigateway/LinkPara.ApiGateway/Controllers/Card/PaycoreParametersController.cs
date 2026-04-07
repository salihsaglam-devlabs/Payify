using LinkPara.ApiGateway.Services.Card.HttpClients;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreParameters.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Card;

public class PaycoreParametersController : ApiControllerBase
{
    private readonly IPaycoreParametersHttpClient _paycoreParametersHttpClient;

    public PaycoreParametersController(IPaycoreParametersHttpClient paycoreParametersHttpClient)
    {
        _paycoreParametersHttpClient = paycoreParametersHttpClient;
    }

    [Authorize(Policy = "PaycoreParameters:ReadAll")]
    [HttpGet("")]
    public async Task<GetProductsResponse> GetProductsAsync()
    {
        return await _paycoreParametersHttpClient.GetProductsAsync();
    }
}