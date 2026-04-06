using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.BankAccounts.Queries.GetBankAccountByMerchantId;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class BankAccountsController : ApiControllerBase
{
    /// <summary>
    /// Returns merchant bank account by merchant id.
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "BankApi:Read")]
    [HttpGet("")]
    public async Task<MerchantBankAccountDto> GetBankAccountByMerchantId([FromQuery] GetBankAccountByMerchantIdQuery request)
    {
        return await Mediator.Send(request);
    }
}
