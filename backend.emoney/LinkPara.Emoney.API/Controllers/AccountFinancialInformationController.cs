using LinkPara.Emoney.Application.Features.AccountFinancialInformations;
using LinkPara.Emoney.Application.Features.AccountFinancialInformations.Commands;
using LinkPara.Emoney.Application.Features.AccountFinancialInformations.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class AccountFinancialInformationController : ApiControllerBase
{
    /// <summary>
    /// Returns an Account Financial Information by account id
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountFinancialInformation:Read")]
    [HttpGet("{accountId}")]
    public async Task<AccountFinancialInfoDto> GetAccountFinancialInfoByAccountIdAsyn([FromRoute] Guid accountId)
    {
        return await Mediator.Send(new GetAccountFinancialInfoQuery { AccountId = accountId });
    }

    /// <summary>
    /// Creates a new Account Financial Information
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "AccountFinancialInformation:Create")]
    [HttpPost]
    public async Task CreateAccountFinancialInfoAsync(CreateAccountFinancialInfoCommand command)
    {
        await Mediator.Send(command);
    }
}
