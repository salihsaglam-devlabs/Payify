using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

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
    [HttpGet("")]
    [Authorize(Policy = "AcquireBank:ReadAll")]
    public async Task<ActionResult<PaginatedList<BankDto>>> GetAllAsync([FromQuery] GetFilterBankRequest request)
    {
        return await _bankHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a bank api key
    /// </summary>
    /// <param name="acquireBankId"></param>
    /// <returns></returns>
    [HttpGet("{acquireBankId}/bank-api-keys")]
    [Authorize(Policy = "AcquireBank:ReadAll")]
    public async Task<ActionResult<List<BankApiKeyDto>>> GetAllBankApiKeyAsync([FromRoute] Guid acquireBankId)
    {
        return await _bankHttpClient.GetAllBankApiKeyAsync(acquireBankId);
    }
}