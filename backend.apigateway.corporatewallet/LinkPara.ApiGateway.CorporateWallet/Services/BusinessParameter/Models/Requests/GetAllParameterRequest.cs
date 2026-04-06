using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.BusinessParameter.Models.Requests
{
    public class GetAllParameterRequest : SearchQueryParams
    {
        public string GroupCode { get; set; }
    }
}
