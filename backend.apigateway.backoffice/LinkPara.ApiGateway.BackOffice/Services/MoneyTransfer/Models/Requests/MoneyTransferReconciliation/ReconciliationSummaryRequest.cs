using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.MoneyTransferReconciliation
{
    public class ReconciliationSummaryRequest : SearchQueryParams   
    {
        public int? BankCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReconciliationSummaryStatus? Status { get; set; }
    }
}
