namespace LinkPara.SharedModels.BusModels.Commands.Emoney;

public class ProcessIncomingTransaction
{
    public Guid IncomingTransactionId { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime TransactionDate { get; set; }
    public string SenderAccountNumber { get; set; }
    public string SenderName { get; set; }
    public int SenderBankCode { get; set; }
    public string SenderBranchCode { get; set; }
    public string SenderTaxNumber { get; set; }
    public string ReceiverAccountNumber { get; set; }
    public string ReceiverName { get; set; }
    public int ReceiverBankCode { get; set; }
    public string ReceiverBranchCode { get; set; }
    public string Description { get; set; }
    public string ErrorMessage { get; set; }
    public string SenderBankName { get; set; }
    public string VirtualIbanNumber { get; set; }
    public string BankReferenceNumber { get; set; }
}
