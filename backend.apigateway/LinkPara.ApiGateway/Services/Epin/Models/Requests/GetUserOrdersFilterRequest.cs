using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Epin.Models.Requests;

public class GetUserOrdersFilterRequest : SearchQueryParams
{
}

public class GetUserOrdersFilterServiceRequest : GetUserOrdersFilterRequest, IHasUserId
{
    public Guid UserId { get; set; }
}