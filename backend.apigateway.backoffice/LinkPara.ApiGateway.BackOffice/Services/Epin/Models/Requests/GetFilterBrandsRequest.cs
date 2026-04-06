using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;

public class GetFilterBrandsRequest : SearchQueryParams
{
    public Guid? PublisherId { get; set; }
}
