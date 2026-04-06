using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses
{
    public class ReturnedTransactionDto
    {
        public Guid Id { get; set; }
        public ReturnedTransactionStatus ReturnStatus { get; set; }
        public DateTime ReturnedDate { get; set; }
        public int BankCode { get; set; }
        public string BankName { get; set; }
        public Guid MoneyTransferTransactionId { get; set; }
        public string IbanNumber { get; set; }
        public string NameSurname { get; set; }
        public DateTime TransactionDate { get; set; }
        public string BankReferenceNumber { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string CurrencyCode { get; set; }
        public string ErrorMessage { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
