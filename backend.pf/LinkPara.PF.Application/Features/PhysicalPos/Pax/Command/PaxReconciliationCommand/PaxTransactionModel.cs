namespace LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxReconciliationCommand;

public class PaxTransactionModel
{
    public string PaymentId { get; set; }
    public string BatchId { get; set; }
    public int Date { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Currency { get; set; }
    public string MerchantId { get; set; }
    public string TerminalId { get; set; }
    public int Amount { get; set; }
    public int PointAmount { get; set; }
    public int Installment { get; set; }
    public string MaskedCardNo { get; set; }
    public string BinNumber { get; set; }
    public string ProvisionNo { get; set; }
    public string AcquirerResponseCode { get; set; }
    public int InstitutionId { get; set; }
    public string Vendor { get; set; }
    public string Rrn { get; set; }
    public string Stan { get; set; }
    public string PosEntryMode { get; set; }
    public string PinEntryInfo { get; set; }
    public string BankRef { get; set; }
    public string OriginalRef { get; set; }
}