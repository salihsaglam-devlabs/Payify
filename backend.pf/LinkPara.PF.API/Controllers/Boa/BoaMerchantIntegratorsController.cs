using LinkPara.PF.Application.Features.MerchantIntegrators;
using LinkPara.PF.Application.Features.MerchantIntegrators.Queries.GetAllMerchantIntegrator;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers.Boa;

public class BoaMerchantIntegratorsController : ApiControllerBase
{
    /// <summary>
    /// Returns all merchant integrators for the boa system.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<PaginatedList<MerchantIntegratorDto>> GetAllMerchantIntegratorsAsync([FromQuery] SearchQueryParams request)
    {
        return await Mediator.Send(new GetAllMerchantIntegratorQuery { SearchQueryParams = request });
    }
}