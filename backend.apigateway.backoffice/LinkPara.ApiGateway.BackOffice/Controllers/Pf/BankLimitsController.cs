using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;
public class BankLimitsController : ApiControllerBase
{
    private readonly IBankLimitHttpClient _bankLimitService;

    public BankLimitsController(IBankLimitHttpClient bankLimitService)
    {
        _bankLimitService = bankLimitService;
    }

    /// <summary>
    /// Returns all bank limits.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "BankLimit:ReadAll")]
    public async Task<ActionResult<PaginatedList<BankLimitDto>>> GetAllAsync([FromQuery] GetFilterBankLimitRequest request)
    {
        return await _bankLimitService.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a bank limit.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "BankLimit:Read")]
    public async Task<ActionResult<BankLimitDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _bankLimitService.GetByIdAsync(id);
    }

    /// <summary>
    /// Creates a bank limit.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "BankLimit:Create")]
    public async Task SaveAsync(SaveBankLimitRequest request)
    {
        await _bankLimitService.SaveAsync(request);
    }

    /// <summary>
    /// Updates a bank limit.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "BankLimit:Update")]
    public async Task UpdateAsync(UpdateBankLimitRequest request)
    {
        await _bankLimitService.UpdateAsync(request);
    }

    /// <summary>
    /// Delete a bank limit.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "BankLimit:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _bankLimitService.DeleteBankLimitAsync(id);
    }
}
