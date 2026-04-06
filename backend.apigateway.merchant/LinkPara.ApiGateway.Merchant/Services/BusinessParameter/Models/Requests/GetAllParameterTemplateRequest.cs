using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Merchant.Services.BusinessParameter.Models.Requests
{
    public class GetAllParameterTemplateRequest : SearchQueryParams
    {
        public string GroupCode { get; set; }
    }
}
