using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.MoneyTransferReconciliation
{
    public class ReconciliationDetailRequest : SearchQueryParams
    {
        public Guid ReconciliationSummaryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? BankCode { get; set; }
        public ReconciliationSummaryStatus? SummaryStatus { get; set; }
        public ReconciliationDetailStatus? DetailStatus { get; set; }
        public BankTransactionType? BankTransactionType { get; set; }
    }
}
