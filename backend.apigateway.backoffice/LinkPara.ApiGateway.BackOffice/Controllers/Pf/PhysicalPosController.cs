using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class PhysicalPosController : ApiControllerBase
{
    private readonly IPhysicalPosHttpClient _physicalPosHttpClient;
    public PhysicalPosController(IPhysicalPosHttpClient physicalPosHttpClient)
    {
        _physicalPosHttpClient = physicalPosHttpClient;
    }

    /// <summary>
    /// Returns all physical pos
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "PhysicalPos:ReadAll")]
    public async Task<ActionResult<PaginatedList<PhysicalPosDto>>> GetAllAsync([FromQuery] GetAllPhysicalPosRequest request)
    {
        return await _physicalPosHttpClient.GetAllAsync(request);
    }

    /// <summary>
    /// Returns a physical pos.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "PhysicalPos:Read")]
    public async Task<ActionResult<PhysicalPosDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _physicalPosHttpClient.GetByIdAsync(id);
    }

    /// <summary>
    /// Creates a physical pos.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("")]
    [Authorize(Policy = "PhysicalPos:Create")]
    public async Task SaveAsync(SavePhysicalPosRequest request)
    {
        await _physicalPosHttpClient.SaveAsync(request);
    }

    /// <summary>
    /// Updates a physical pos.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "PhysicalPos:Update")]
    public async Task UpdateAsync(UpdatePhysicalPosRequest request)
    {
        await _physicalPosHttpClient.UpdateAsync(request);
    }

    /// <summary>
    /// Delete a physical pos.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("")]
    [Authorize(Policy = "PhysicalPos:Delete")]
    public async Task DeleteAsync(Guid id)
    {
        await _physicalPosHttpClient.DeletePhysicalPosAsync(id);
    }

    /// <summary>
    /// Returns a physical pos.
    /// </summary>
    /// <param name="bkmReferenceNumber"></param>
    /// <returns></returns>
    [Authorize(Policy = "PhysicalPos:Read")]
    [HttpGet("bkm/{bkmReferenceNumber}")]
    public async Task<ActionResult<MerchantPhysicalPosDto>> GetByReferenceNumberAsync([FromRoute] string bkmReferenceNumber)
    {
        return await _physicalPosHttpClient.GetByReferenceNumberAsync(bkmReferenceNumber);
    }
}
