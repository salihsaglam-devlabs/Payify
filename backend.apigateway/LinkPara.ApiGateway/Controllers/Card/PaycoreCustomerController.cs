using LinkPara.ApiGateway.Services.Card.HttpClients;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Request;
using LinkPara.ApiGateway.Services.Card.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Card;

public class PaycoreCustomerController : ApiControllerBase
{
    private readonly IPaycoreCustomerHttpClient _paycoreCustomerHttpClient;

    public PaycoreCustomerController(IPaycoreCustomerHttpClient paycoreCustomerHttpClient)
    {
        _paycoreCustomerHttpClient = paycoreCustomerHttpClient;
    }

    [Authorize(Policy = "PaycoreCustomer:Create")]
    [HttpPost("")]
    public async Task<PaycoreResponse> CreateCustomerAsync([FromBody] CreateCustomerRequest request)
    {
        return await _paycoreCustomerHttpClient.CreateCustomerAsync(request);
    }
}