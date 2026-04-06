using LinkPara.Emoney.Application.Features.Currencies;
using LinkPara.Emoney.Application.Features.Currencies.Queries.Currencies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class CurrenciesController : ApiControllerBase
{
    /// <summary>
    /// Returns all currencies list.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "EmoneyCurrency:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<List<CurrencyDto>>> GetAllAsync([FromQuery] CurrenciesFilterQuery query)
    {
        return await Mediator.Send(query);
    }
}