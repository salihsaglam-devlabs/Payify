using LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Pf;

public class MerchantCategoryCodesController : ApiControllerBase
{
    private readonly IMccHttpClient _mccHttpClient;

    public MerchantCategoryCodesController(IMccHttpClient mccHttpClient)
    {
        _mccHttpClient = mccHttpClient;
    }

    /// <summary>
    /// Returns all mcc.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<MccDto>>> GetAllAsync([FromQuery] GetFilterMccRequest request)
    {
        return await _mccHttpClient.GetAllAsync(request);
    }
}