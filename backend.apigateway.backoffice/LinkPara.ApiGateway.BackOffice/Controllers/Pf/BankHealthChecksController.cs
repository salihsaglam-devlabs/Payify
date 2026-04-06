using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class BankHealthChecksController : ApiControllerBase
{
    private readonly IBankHealthCheckHttpClient _bankHealthCheckHttpClient;

    public BankHealthChecksController(IBankHealthCheckHttpClient bankHealthCheckHttpClient)
    {
        _bankHealthCheckHttpClient = bankHealthCheckHttpClient;
    }

    /// <summary>
    /// Returns all health checks.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "BankHealthCheck:ReadAll")]
    public async Task<ActionResult<PaginatedList<BankHealthCheckDto>>> GetAllAsync([FromQuery] GetFilterBankHealthCheckRequest request)
    {
        return await _bankHealthCheckHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Updates a health check.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "BankHealthCheck:Update")]
    public async Task UpdateAsync(UpdateBankHealthCheckRequest request)
    {
        await _bankHealthCheckHttpClient.UpdateAsync(request);
    }
}
