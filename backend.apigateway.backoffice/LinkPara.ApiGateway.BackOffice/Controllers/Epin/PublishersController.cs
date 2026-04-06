using LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Epin;

public class PublishersController : ApiControllerBase
{
    private readonly IPublisherHttpClient _httpClient;

    public PublishersController(IPublisherHttpClient httpClient)
    {
        _httpClient = httpClient;
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
        return await _httpClient.GetFilterPublishersAsync(request);
    }
}
