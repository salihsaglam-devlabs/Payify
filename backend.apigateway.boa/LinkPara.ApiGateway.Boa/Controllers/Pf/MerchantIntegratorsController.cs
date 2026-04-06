using LinkPara.ApiGateway.Boa.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Boa.Controllers.Pf;

public class MerchantIntegratorsController : ApiControllerBase
{
    private readonly IMerchantIntegratorHttpClient _merchantIntegratorHttpClient;

    public MerchantIntegratorsController(IMerchantIntegratorHttpClient merchantIntegratorHttpClient)
    {
        _merchantIntegratorHttpClient = merchantIntegratorHttpClient;
    }

    /// <summary>
    /// Returns all merchant integrators
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<MerchantIntegratorDto>>> GetAllAsync([FromQuery] SearchQueryParams request)
    {
        return await _merchantIntegratorHttpClient.GetAllAsync(request);
    }
}