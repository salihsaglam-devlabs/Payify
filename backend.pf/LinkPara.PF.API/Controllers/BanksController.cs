using LinkPara.PF.Application.Features.Banks;
using LinkPara.PF.Application.Features.Banks.Queries.GetAllBank;
using LinkPara.PF.Application.Features.Banks.Queries.GetBankApiKey;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class BanksController : ApiControllerBase
{
    /// <summary>
    /// Returns a banks
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<BankDto>>> GetAllAsync([FromQuery] GetAllBankQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a bank api key
    /// </summary>
    /// <param name="acquireBankId"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("{acquireBankId}/bank-api-keys")]
    public async Task<ActionResult<List<BankApiKeyDto>>> GetBankApiKeyAsync([FromRoute] Guid acquireBankId)
    {
        return await Mediator.Send(new GetBankApiKeyQuery { AcquireBankId = acquireBankId });
    }
}
