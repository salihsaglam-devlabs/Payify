using LinkPara.ApiGateway.BackOffice.Services.Billing.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Billing.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Billing;

public class VendorsController : ApiControllerBase
{
    private readonly IVendorHttpClient _vendorHttpClient;

    public VendorsController(IVendorHttpClient vendorHttpClient)
    {
        _vendorHttpClient = vendorHttpClient;
    }

    /// <summary>
    /// get all vendors
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Vendor:ReadAll")]
    public async Task<ActionResult<PaginatedList<VendorDto>>> GetAllVendorAsync([FromQuery] VendorFilterRequest request)
    {
        return await _vendorHttpClient.GetAllVendorAsync(request);
    }
}