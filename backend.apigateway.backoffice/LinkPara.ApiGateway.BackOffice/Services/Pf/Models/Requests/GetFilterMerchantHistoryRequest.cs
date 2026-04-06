using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class GetFilterMerchantHistoryRequest : SearchQueryParams
    {
        public DateTime? CreateDateStart { get; set; }
        public DateTime? CreateDateEnd { get; set; }
        public PermissionOperationType? PermissionOperationType { get; set; }
        public string MerchantName { get; set; }
        public string CreatedNameBy { get; set; }
    }
}
