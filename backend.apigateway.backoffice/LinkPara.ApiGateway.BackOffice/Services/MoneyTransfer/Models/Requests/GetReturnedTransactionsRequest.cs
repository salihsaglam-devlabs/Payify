using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests
{
    public class GetReturnedTransactionsRequest : SearchQueryParams
    {
        public Guid? Id { get; set; }
        public Guid? MoneyTransferTransactionId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BankCode { get; set; }
        public ReturnedTransactionStatus? ReturnStatus { get; set; }
    }
}
