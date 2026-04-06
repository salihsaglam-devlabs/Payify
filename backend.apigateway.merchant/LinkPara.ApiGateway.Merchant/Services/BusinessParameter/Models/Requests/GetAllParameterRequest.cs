using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.BusinessParameter.Models.Requests
{
    public class GetAllParameterRequest : SearchQueryParams
    {
        public string GroupCode { get; set; }
    }
}
