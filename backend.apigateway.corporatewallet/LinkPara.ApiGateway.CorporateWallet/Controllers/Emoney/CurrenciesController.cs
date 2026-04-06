using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Controllers.Emoney;

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
    [HttpGet("")]
    public async Task<ActionResult<List<CurrencyDto>>> GetAllAsync([FromQuery] CurrenciesFilterRequest request)
    {
        return await _currencyHttpClient.GetAllAsync(request);
    }
}
