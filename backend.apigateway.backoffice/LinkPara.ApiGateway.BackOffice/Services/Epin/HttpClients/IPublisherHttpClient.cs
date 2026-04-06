using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.HttpClients;

public interface IPublisherHttpClient
{
    Task<ActionResult<PaginatedList<PublisherDto>>> GetFilterPublishersAsync(GetFilterPublishersRequest request);
}
