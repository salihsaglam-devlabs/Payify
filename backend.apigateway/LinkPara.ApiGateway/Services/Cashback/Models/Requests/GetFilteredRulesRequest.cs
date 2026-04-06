using LinkPara.ApiGateway.Services.Cashback.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Cashback.Models.Requests
{
    public class GetFilteredRulesRequest : SearchQueryParams
    {
        public CashbackProcessType? ProcessType { get; set; }
        public DateTime? RuleStartDate { get; set; }
        public DateTime? RuleEndDate { get; set; }
        public RecordStatus? RecordStatus { get; set; }
    }
}