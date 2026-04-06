using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney;

public class CurrenciesController : ApiControllerBase
{
    private readonly ICurrencyHttpClient _currencyHttpClient;

    public CurrenciesController(ICurrencyHttpClient currencyHttpClient)
    {
        _currencyHttpClient = currencyHttpClient;
    }

    /// <summary>
    /// Returns all currencies list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyCurrency:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<List<CurrencyDto>>> GetAllAsync([FromQuery] CurrenciesFilterRequest request)
    {
        return await _currencyHttpClient.GetAllAsync(request);
    }
}