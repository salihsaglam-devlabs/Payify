using LinkPara.Billing.Application.Features.Commissions;
using LinkPara.Billing.Application.Features.Commissions.Commands.CreateCommission;
using LinkPara.Billing.Application.Features.Commissions.Commands.DeleteCommission;
using LinkPara.Billing.Application.Features.Commissions.Commands.SaveCommission;
using LinkPara.Billing.Application.Features.Commissions.Queries.GetAllCommission;
using LinkPara.Billing.Application.Features.Commissions.Queries.GetByDetail;
using LinkPara.Billing.Application.Features.Commissions.Queries.GetById;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Billing.API.Controllers;

public class CommissionsController : ApiControllerBase
{
    /// <summary>
    /// get all billing commissions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<CommissionDto>> GetAllAsync([FromQuery] GetAllCommissionQuery request)
    {
        return await Mediator.Send(request);
    }
    
    /// <summary>
    ///  Get commission by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:Read")]
    [HttpGet("{id}")]
    public async Task<CommissionDto> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetCommissionByIdQuery { CommissionId = id });
    }

    /// <summary>
    /// add new commission
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:Create")]
    [HttpPost("")]
    public async Task AddAsync([FromBody] CreateCommissionQuery request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// delete existing commission
    /// </summary>
    /// <param name="id"></param>
    [Authorize(Policy = "BillingCommission:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync([FromRoute] Guid id)
    {
        await Mediator.Send(new DeleteCommissionCommand { CommissionId = id });
    }

    /// <summary>
    /// update existing commission
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:Update")]
    [HttpPut("")]
    public async Task UpdateAsync([FromBody] SaveCommissionCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// get commissions for institution
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:Read")]
    [HttpGet("by-detail")]
    public async Task<CommissionDto> GetByDetailAsync([FromQuery] GetByDetailQuery request)
    {
        return await Mediator.Send(request);
    }
}