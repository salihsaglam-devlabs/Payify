using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Emoney.Domain.Entities;

public class ReturnTransactionRequest : AuditEntity
{
    public ReturnTransactionStatus Status { get; set; }
    public TransferType TransferType { get; set; }
    public string CurrencyCode { get; set; }
    public string ReceiverIbanNumber { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverTaxNumber { get; set; }
    public int ReceiverBankCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public Guid IncomingTransactionId { get; set; }
    public int TransactionBankCode { get; set; }
    public DateTime MoneyTransferPaymentDate { get; set; }
    public MoneyTransferStatus MoneyTransferStatus { get; set; }
    public Guid MoneyTransferReferenceId { get; set; }
    public bool IsProcessed { get; set; }
}