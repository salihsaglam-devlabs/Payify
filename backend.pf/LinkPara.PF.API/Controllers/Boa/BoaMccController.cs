using LinkPara.PF.Application.Features.MerchantCategoryCodes;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetAllMcc;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers.Boa;

public class BoaMccController : ApiControllerBase
{
    /// <summary>
    /// Returns all mcc for the boa system.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<PaginatedList<MccDto>> GetAllMccAsync([FromQuery] GetAllMccQuery request)
    {
        return await Mediator.Send(request);
    }
}