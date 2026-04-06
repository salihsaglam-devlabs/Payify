using LinkPara.Billing.Application.Features.Vendors;
using LinkPara.Billing.Application.Features.Vendors.Commands;
using LinkPara.Billing.Application.Features.Vendors.Queries.GetAllVendor;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Billing.API.Controllers;

public class VendorsController : ApiControllerBase
{
    /// <summary>
    /// get all vendor
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vendor:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<VendorDto>> GetAllAsync([FromQuery] GetAllVendorQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// save new vendor
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Vendor:Create")]
    [HttpPost("")]
    public async Task SaveAsync([FromBody] SaveVendorCommand command)
    {
        await Mediator.Send(command);
    }
}
