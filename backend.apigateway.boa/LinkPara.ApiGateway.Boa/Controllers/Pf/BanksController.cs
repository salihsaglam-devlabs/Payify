using LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Pf;

public class BanksController : ApiControllerBase
{
    private readonly IBankHttpClient _bankHttpClient;

    public BanksController(IBankHttpClient bankHttpClient)
    {
        _bankHttpClient = bankHttpClient;
    }

    /// <summary>
    /// Returns all banks
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<BankDto>>> GetAllAsync([FromQuery] GetFilterBankRequest request)
    {
        return await _bankHttpClient.GetAllBanksAsync(request);
    }
}