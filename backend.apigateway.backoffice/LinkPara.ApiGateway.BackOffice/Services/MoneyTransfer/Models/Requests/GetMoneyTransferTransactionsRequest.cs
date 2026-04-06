using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests
{
    public class GetMoneyTransferTransactionsRequest : SearchQueryParams
    {
        public Guid? Id { get; set; }
        public MoneyTransferStatus? Status { get; set; }
        public TransactionSource? Source { get; set; }
        public TransferType? TransferType { get; set; }
        public RecordStatus? RecordStatus { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string CurrencyCode { get; set; }
        public string ReceiverIbanNumber { get; set; }
        public string ReceiverName { get; set; }
        public string Description { get; set; }
        public int? IbanBankCode { get; set; }
        public int? TransferBankCode { get; set; }
    }
}
