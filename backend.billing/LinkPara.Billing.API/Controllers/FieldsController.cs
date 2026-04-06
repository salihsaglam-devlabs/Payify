using LinkPara.Billing.Application.Features.Fields;
using LinkPara.Billing.Application.Features.Fields.Queries;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Billing.API.Controllers;

public class FieldsController : ApiControllerBase
{
    /// <summary>
    /// get fields by institution id
    /// </summary>
    /// <param name="institutionId"></param>
    /// <returns></returns>
    [Authorize(Policy = "Institution:ReadAll")]
    [HttpGet("{institutionId}")]
    public async Task<PaginatedList<FieldDto>> GetByInstitutionIdAsync([FromRoute] Guid institutionId)
    {
        return await Mediator.Send(new GetByInstitutionIdQuery { InstitutionId = institutionId });
    }
}