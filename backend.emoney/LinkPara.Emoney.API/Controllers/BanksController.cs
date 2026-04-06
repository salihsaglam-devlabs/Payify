using LinkPara.Emoney.Application.Features.Banks;
using LinkPara.Emoney.Application.Features.Banks.Queries.GetBanksList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class BanksController : ApiControllerBase
{
    /// <summary>
    /// Returns all bank list.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Bank:ReadAll")]
    [HttpGet("")]
    public async Task<List<BankDto>> GetBanksAsync([FromQuery] GetBanksListQuery query)
    {
        return await Mediator.Send(query);
    }
}