using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;

public class GetFilterProductsRequest : SearchQueryParams
{
    public Guid PublisherId { get; set; }
    public Guid BrandId { get; set; }
}
