using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class GetFilterDueProfileRequest : SearchQueryParams
    {
        public string Title { get; set; }
        public DueType? DueType { get; set; }
        public decimal? AmountBiggerThan { get; set; }
        public decimal? AmountSmallerThan { get; set; }
        public int? Currency { get; set; }
        public TimeInterval? OccurenceInterval { get; set; }
        public bool? IsDefault { get; set; }
        public RecordStatus? RecordStatus { get; set; }
    }
}
