using LinkPara.Emoney.Application.Features.SystemBankAccounts;
using LinkPara.Emoney.Application.Features.SystemBankAccounts.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class SystemBankAccountsController : ApiControllerBase
{
    [Authorize(Policy = "Bank:ReadAll")]
    [HttpGet("")]
    public async Task<List<SystemBankAccountDto>> GetSystemBankAccountsAsync([FromQuery] GetSystemBankAccountsQuery query)
    {
        return await Mediator.Send(query);
    }
}


