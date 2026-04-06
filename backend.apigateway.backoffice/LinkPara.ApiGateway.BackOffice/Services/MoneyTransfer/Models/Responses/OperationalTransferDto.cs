using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses;

public class OperationalTransferDto
{
    public decimal Amount { get; set; }
    public MoneyTransferStatus Status { get; set; }
    public TransferType TransferType { get; set; }
    public string CurrencyCode { get; set; }
    public int SenderBankCode { get; set; }
    public string SenderBankName { get; set; }
    public string SenderIbanNumber { get; set; }
    public int ReceiverBankCode { get; set; }
    public string ReceiverBankName { get; set; }
    public string ReceiverIbanNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal ReceiverCurrentBalance { get; set; }
    public string Description { get; set; }
}
