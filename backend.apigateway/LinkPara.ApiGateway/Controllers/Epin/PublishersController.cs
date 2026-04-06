using LinkPara.ApiGateway.Services.Epin.HttpClients;
using LinkPara.ApiGateway.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Epin;

public class PublishersController : ApiControllerBase
{
    private readonly IEpinHttpClient _epinHttpClient;

    public PublishersController(IEpinHttpClient epinHttpClient)
    {
        _epinHttpClient = epinHttpClient;
    }

    /// <summary>
    /// get all publishers list
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "EpinPublisher:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<PublisherDto>>> GetFilterPublishersAsync([FromQuery] GetFilterPublishersRequest request)
    {
        return await _epinHttpClient.GetFilterPublishersAsync(request);
    }

}
