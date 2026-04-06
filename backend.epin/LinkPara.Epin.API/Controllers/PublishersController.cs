using LinkPara.Epin.Application.Features.Publishers;
using LinkPara.Epin.Application.Features.Publishers.Queries;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Epin.API.Controllers;

public class PublishersController : ApiControllerBase
{
    /// <summary>
    /// Get Publishers List
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinPublisher:ReadAll")]
    [HttpGet("")]
    public async Task<PaginatedList<PublisherDto>> GetFilterPublishersAsync([FromQuery] GetFilterPublishersQuery request)
    {
        return await Mediator.Send(request);
    }
}
