using LinkPara.ApiGateway.Services.Billing.HttpClients;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Billing;

public class FieldsController : ApiControllerBase
{
    private readonly IFieldHttpClient _fieldHttpClient;

    public FieldsController(IFieldHttpClient fieldHttpClient)
    {
        _fieldHttpClient = fieldHttpClient;
    }

    /// <summary>
    /// gets list of fields by institution id
    /// </summary>
    /// <param name="institutionId"></param>
    /// <returns></returns>
    [Authorize(Policy = "Institution:ReadAll")]
    [HttpGet("{institutionId}")]
    public async Task<ActionResult<PaginatedList<FieldDto>>> GetByInstitutionIdAsync([FromRoute] Guid institutionId)
    {
        return await _fieldHttpClient.GetByInstitutionIdAsync(institutionId);
    }
}