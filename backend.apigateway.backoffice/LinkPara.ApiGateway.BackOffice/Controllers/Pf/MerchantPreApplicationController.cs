using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class MerchantPreApplicationController : ApiControllerBase
{
    private readonly IMerchantPreApplicationHttpClient _merchantPreApplicationHttpClient;

    public MerchantPreApplicationController(IMerchantPreApplicationHttpClient merchantPreApplicationHttpClient)
    {
        _merchantPreApplicationHttpClient = merchantPreApplicationHttpClient;
    }
    /// <summary>
    /// Returns a merchant pre application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "PreApplication:Read")]
    public async Task<ActionResult<MerchantPreApplicationDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _merchantPreApplicationHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Returns filtered merchant pre applications
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "PreApplication:ReadAll")]
    public async Task<ActionResult<PaginatedList<MerchantPreApplicationDto>>> GetFilterAsync(
        [FromQuery] GetAllMerchantPreApplicationsRequest request)
    {
        return await _merchantPreApplicationHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Updates a merchant pre application
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "PreApplication:Update")]
    public async Task UpdateAsync(UpdateMerchantPreApplicationRequest request)
    { 
        await _merchantPreApplicationHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Delete merchant pre application
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "PreApplication:Delete")]
    public async Task DeleteAsync([FromRoute] Guid id)
    {
        await _merchantPreApplicationHttpClient.DeleteMerchantApplicationAsync(id);
    }
}