using LinkPara.Emoney.Application.Features.CompanyPools;
using LinkPara.Emoney.Application.Features.CompanyPools.Commands.ApproveCompanyPool;
using LinkPara.Emoney.Application.Features.CompanyPools.Commands.SaveCompanyPool;
using LinkPara.Emoney.Application.Features.CompanyPools.Queries.GetCompanyDocumentTypeList;
using LinkPara.Emoney.Application.Features.CompanyPools.Queries.GetCompanyPoolById;
using LinkPara.Emoney.Application.Features.CompanyPools.Queries.GetCompanyPoolList;
using LinkPara.Emoney.Application.Features.CompanyPools.Queries.GetTaxAdministirations;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

public class CompanyPoolsController : ApiControllerBase
{
    /// <summary>
    /// save company pool
    /// </summary>
    /// <returns></returns>
    [HttpPost("")]
    [AllowAnonymous]
    public async Task<SaveCompanyPoolResponse> SaveAsync([FromForm]SaveCompanyPoolCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Get Tax Administrations
    /// </summary>
    /// <returns></returns>
    [HttpGet("tax-administrations")]
    [AllowAnonymous]
    public async Task<List<TaxAdministrationsResponse>> GetTaxAdministrationsAsync()
    {
        return await Mediator.Send(new GetTaxAdministrationsQuery());
    }

    /// <summary>
    /// Get DocumentTypes
    /// </summary>
    /// <returns></returns>
    [HttpGet("document-types")]
    [AllowAnonymous]
    public async Task<List<CompanyDocumentTypeDto>> GetDocumentTypesAsync([FromQuery] GetCompanyDocumentTypeListQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Get CompanyPools
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "CompanyPool:ReadAll")]
    public async Task<PaginatedList<CompanyPoolDto>> GetCompanyPoolsList([FromQuery] GetCompanyPoolListQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Get CompanyPool By Id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "CompanyPool:Read")]
    public async Task<CompanyPoolDto> GetCompanyPoolById([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetCompanyPoolByIdQuery { Id = id, LoadDocument = true });
    }

    /// <summary>
    /// Approve Company Pool
    /// </summary>
    /// <returns></returns>
    [HttpPut("approve")]
    [Authorize(Policy = "CompanyPool:Update")]
    public async Task<Unit> ApproveCompanyPoolAsync([FromBody] ApproveCompanyPoolCommand command)
    {
        return await Mediator.Send(command);
    }
}
