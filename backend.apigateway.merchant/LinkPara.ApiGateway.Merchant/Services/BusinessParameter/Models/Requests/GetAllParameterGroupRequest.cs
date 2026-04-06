using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.BusinessParameter.Models.Requests
{
    public class GetAllParameterGroupRequest : SearchQueryParams
    {
        public string GroupCode { get; set; }
        public RecordStatus? RecordStatus { get; set; }
    }
}
