using LinkPara.ApiGateway.Services.Card.HttpClients;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Request;
using LinkPara.ApiGateway.Services.Card.Models.PaycoreCard.Response;
using LinkPara.ApiGateway.Services.Card.Models.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Card;

public class PaycoreCardController : ApiControllerBase
{
    private readonly IPaycoreCardHttpClient _paycoreCardHttpClient;

    public PaycoreCardController(IPaycoreCardHttpClient paycoreCardHttpClient)
    {
        _paycoreCardHttpClient = paycoreCardHttpClient;
    }

    [Authorize(Policy = "PaycoreCard:Create")]
    [HttpPost("")]
    public async Task<PaycoreResponse> CreateCardAsync([FromBody] CreateCardRequest request)
    {
        return await _paycoreCardHttpClient.CreateCardAsync(request);
    }

    [Authorize(Policy = "PaycoreCard:Update")]
    [HttpPut("status")]
    public async Task<UpdateCardStatusResponse> UpdateCardStatusAsync([FromBody] UpdateCardStatusRequest request)
    {
        return await _paycoreCardHttpClient.UpdateCardStatusAsync(request);
    }

    [Authorize(Policy = "PaycoreCard:Read")]
    [HttpGet("card-authorizations")]
    public async Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync([FromQuery] GetCardAuthorizationsRequest request)
    {
        return await _paycoreCardHttpClient.GetCardAuthorizationsAsync(request);
    }

    [Authorize(Policy = "PaycoreCard:Read")]
    [HttpGet("card-info")]
    public async Task<List<GetCardInformationsResponse>> GetCardInformationsAsync([FromQuery] GetCardInformationsRequest request)
    {
        return await _paycoreCardHttpClient.GetCardInformationsAsync(request);
    }

    [Authorize(Policy = "PaycoreCard:Update")]
    [HttpPut("card-authorization")]
    public async Task<PaycoreResponse> UpdateCardAuthorizationsAsync([FromBody] UpdateCardAuthorizationRequest request)
    {
        return await _paycoreCardHttpClient.UpdateCardAuthorizationsAsync(request);
    }

    [Authorize(Policy = "PaycoreCard:Read")]
    [HttpGet("card-last-courier-activity")]
    public async Task<GetCardLastCourierActivityResponse> GetCardLastCourierActivityAsync([FromQuery] GetCardLastCourierActivityRequest request)
    {
        return await _paycoreCardHttpClient.GetCardLastCourierActivityAsync(request);
    }

    [Authorize(Policy = "PaycoreCard:Create")]
    [HttpPut("additional-limit-restriction")]
    public async Task<AddAdditionalLimitRestrictionResponse> AddAdditionalLimitRestrictionAsync([FromBody] AddAdditionalLimitRestrictionRequest request)
    {
        return await _paycoreCardHttpClient.AddAdditionalLimitRestrictionAsync(request);
    }

    [Authorize(Policy = "PaycoreCard:Read")]
    [HttpGet("card-sensitive-data")]
    public async Task<GetCardSensitiveDataResponse> GetCardSensitiveDataAsync([FromQuery] GetCardSensitiveDataRequest request)
    {
        return await _paycoreCardHttpClient.GetCardSensitiveDataAsync(request);
    }
}