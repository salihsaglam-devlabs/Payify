using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses
{
    public class MoneyTransferTransactionsDto
    {
        public Guid Id { get; set; }
        public MoneyTransferStatus Status { get; set; }
        public TransactionSource Source { get; set; }
        public TransferType TransferType { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string ReceiverIbanNumber { get; set; }
        public string ReceiverName { get; set; }
        public string Description { get; set; }
        public int IbanBankCode { get; set; }
        public string IbanBankName { get; set; }
        public int TransferBankCode { get; set; }
        public string TransferBankName { get; set; }
        public decimal Cost { get; set; }
        public string ReferenceId { get; set; }
        public int TransactionReferenceId { get; set; }
        public Guid TransactionReferenceGuid { get; set; }
        public string BankReferenceNumber { get; set; }
        public string BankTransactionId { get; set; }
        public string BankQueryNumber { get; set; }
        public string BankTransactionResponse { get; set; }
        public bool IsReturnPayment { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime? QueueDate { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
