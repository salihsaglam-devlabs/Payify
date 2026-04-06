using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request
{
    public class GetAllParameterGroupRequest : SearchQueryParams
    {
        public string GroupCode { get; set; }
        public RecordStatus? RecordStatus { get; set; }
    }
}
