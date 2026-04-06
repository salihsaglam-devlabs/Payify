using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Request
{
    public class GetAllSearchesRequest : SearchQueryParams
    {
        public string SearchName { get; set; }
        public SearchType? SearchType { get; set; }
        public MatchStatus? MatchStatus { get; set; }
        public bool? IsBlackList { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}
