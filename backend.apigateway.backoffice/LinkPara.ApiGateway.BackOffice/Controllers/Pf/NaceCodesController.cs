using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf;

public class NaceCodesController : ApiControllerBase
{
    private readonly INaceCodesHttpClient _naceCodesHttpClient;

    public NaceCodesController(INaceCodesHttpClient naceCodesHttpClient)
    {
        _naceCodesHttpClient = naceCodesHttpClient;
    }
    
    /// <summary>
    /// Returns all transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "NaceCode:ReadAll")]
    public async Task<ActionResult<PaginatedList<NaceDto>>> GetAllAsync([FromQuery] GetAllNaceCodesRequest request)
    {
        return await _naceCodesHttpClient.GetAllAsync(request);
    }
    
    /// <summary>
    /// Return a transaction
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "NaceCode:Read")]
    public async Task<ActionResult<NaceDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await _naceCodesHttpClient.GetByIdAsync(id);
    }
}