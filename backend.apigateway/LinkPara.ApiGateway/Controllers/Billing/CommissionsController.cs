using LinkPara.ApiGateway.Services.Billing.HttpClients;
using LinkPara.ApiGateway.Services.Billing.Models.Requests;
using LinkPara.ApiGateway.Services.Billing.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Billing;

public class CommissionsController : ApiControllerBase
{
    private readonly ICommissionHttpClient _commissionHttpClient;

    public CommissionsController(ICommissionHttpClient commissionHttpClient)
    {
        _commissionHttpClient = commissionHttpClient;
    }

    /// <summary>
    /// get commission for given institution
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "BillingCommission:Read")]
    [HttpGet("")]
    public async Task<CommissionDto> GetByDetailAsync([FromQuery] CommissionFilterRequest request)
    {
        return await _commissionHttpClient.GetByDetailAsync(request);
    }
}
