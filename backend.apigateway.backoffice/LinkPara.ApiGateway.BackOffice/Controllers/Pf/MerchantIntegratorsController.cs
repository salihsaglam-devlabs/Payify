using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantIntegratorsController : ApiControllerBase
{
    private readonly IMerchantIntegratorHttpClient _merchantIntegratorHttpClient;

    public MerchantIntegratorsController(IMerchantIntegratorHttpClient merchantIntegratorHttpClient)
    {
        _merchantIntegratorHttpClient = merchantIntegratorHttpClient;
    }

    /// <summary>
    /// Returns all merchant integrators
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "MerchantIntegrator:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantIntegratorDto>>> GetAllAsync([FromQuery] SearchQueryParams request)
    {
        return await _merchantIntegratorHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a merchant integrator
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "MerchantIntegrator:Read")]
    public async Task<ActionResult<MerchantIntegratorDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _merchantIntegratorHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Creates a merchant integrator
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "MerchantIntegrator:Create")]
    public async Task SaveAsync(SaveMerchantIntegratorRequest request)
    {
        await _merchantIntegratorHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates a merchant integrator
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "MerchantIntegrator:Update")]
    public async Task UpdateAsync(UpdateMerchantIntegratorRequest request)
    {
        await _merchantIntegratorHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Delete a merchant integrator
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "MerchantIntegrator:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _merchantIntegratorHttpClient.DeleteMerchantIntegratorAsync(id);
    }
}
