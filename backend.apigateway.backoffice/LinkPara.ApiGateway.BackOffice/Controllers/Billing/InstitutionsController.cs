using LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Billing;

public class InstitutionsController : ApiControllerBase
{
    private readonly IInstitutionHttpClient _institutionHttpClient;

    public InstitutionsController(IInstitutionHttpClient institutionHttpClient)
    {
        _institutionHttpClient = institutionHttpClient;
    }

    /// <summary>
    /// get all institutions list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Institution:ReadAll")]
    public async Task<ActionResult<PaginatedList<InstitutionDto>>> GetAllInstitutionAsync([FromQuery] InstitutionFilterRequest request)
    {
        return await _institutionHttpClient.GetAllInstitutionAsync(request);
    }

    /// <summary>
    /// get institution by id
    /// </summary>
    /// <param name="institutionId"></param>
    /// <returns></returns>
    [HttpGet("{institutionId}")]
    [Authorize(Policy = "Institution:Read")]
    public async Task<ActionResult<InstitutionDto>> GetByIdAsync([FromRoute] Guid institutionId)
    {
        return await _institutionHttpClient.GetByIdAsync(institutionId);
    }

    /// <summary>
    /// update given institution
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "Institution:Update")]
    public async Task UpdateAsync(UpdateInstitutionRequest request)
    {
        await _institutionHttpClient.UpdateAsync(request);
    }
}