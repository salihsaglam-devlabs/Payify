using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests
{
    public class GetIncomingTransactionsRequest : SearchQueryParams
    {
        public Guid? Id { get; set; }
        public Guid? MoneyTransferTransactionId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public IncomingTransactionStatus? Status { get; set; }
        public int? ReceiverBankCode { get; set; }
        public int? SenderBankCode { get; set; }
        public IncomingTransferType? IncomingTransferType { get; set; }
        public DateTime? TransactionStartDate { get; set; }
        public DateTime? TransactionEndDate { get; set; }
    }
}
