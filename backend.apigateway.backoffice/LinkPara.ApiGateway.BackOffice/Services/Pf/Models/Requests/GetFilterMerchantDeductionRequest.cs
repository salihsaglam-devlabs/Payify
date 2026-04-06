using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class GetFilterMerchantDeductionRequest : SearchQueryParams
    {
        public Guid MerchantTransactionId { get; set; }
        public Guid MerchantId { get; set; }
        public int? Currency { get; set; }
        public decimal? TotalDeductionAmountBiggerThan { get; set; }
        public decimal? TotalDeductionAmountSmallerThan { get; set; }
        public decimal? RemainingDeductionAmountBiggerThan { get; set; }
        public decimal? RemainingDeductionAmountSmallerThan { get; set; }
        public DateTime? ExecutionDateStart { get; set; }
        public DateTime? ExecutionDateEnd { get; set; }
        public DeductionType? DeductionType { get; set; }
        public DeductionStatus? DeductionStatus { get; set; }
        public Guid MerchantDueId { get; set; }
        public RecordStatus? RecordStatus { get; set; }
        public string ConversationId { get; set; }
    }
}
