using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Pf;

public class AcquireBanksController : ApiControllerBase
{
    private readonly IAcquireBankHttpClient _acquireBankHttpClient;

    public AcquireBanksController(IAcquireBankHttpClient acquireBankHttpClient)
    {
        _acquireBankHttpClient = acquireBankHttpClient;
    }

    /// <summary>
    /// Returns all acquire banks.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "AcquireBank:ReadAll")]
    public async Task<ActionResult<PaginatedList<AcquireBankDto>>> GetAllAsync([FromQuery] GetFilterAcquireBankRequest request)
    {
        return await _acquireBankHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns an acquire bank.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "AcquireBank:Read")]
    public async Task<ActionResult<AcquireBankDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _acquireBankHttpClient.GetByIdAsync(id);
    }
}
