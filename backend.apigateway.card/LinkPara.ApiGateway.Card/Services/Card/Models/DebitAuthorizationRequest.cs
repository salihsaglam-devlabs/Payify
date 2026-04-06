namespace LinkPara.ApiGateway.Card.Services.Card.Models;
public class DebitAuthorizationRequest
{
    public long CorrelationID { get; set; }
    public long OceanTxnGUID { get; set; }
    public string BankingCustomerNo { get; set; } = default!;
    public string CardNo { get; set; }
    public string AccountNo { get; set; }
    public string AccountBranch { get; set; }
    public string AccountSuffix { get; set; }
    public int? AccountCurrency { get; set; }
    public string Iban { get; set; }
    public string AcquirerCountryCode { get; set; }
    public string NationalSwitchId { get; set; }
    public string AcquirerId { get; set; }
    public string TerminalId { get; set; }
    public string MerchantId { get; set; }
    public string MerchantName { get; set; }
    public string Rrn { get; set; }
    public string ProvisionCode { get; set; }
    public TransactionAmount TransactionAmount { get; set; }
    public TransactionAmount BillingAmount { get; set; }
    public TransactionAmount ReplacementTransactionAmount { get; set; }
    public TransactionAmount ReplacementBillingAmount { get; set; }
    public List<Fee> FeeList { get; set; }
    public long RequestDate { get; set; }
    public long RequestTime { get; set; }
    public string Mcc { get; set; }
    public bool IsSimulation { get; set; }
    public bool IsAdvice { get; set; }
    public string RequestType { get; set; }
    public string TransactionType { get; set; }
    public int? ExpirationTime { get; set; }
    public string Channel { get; set; }
    public string TerminalType { get; set; }
    public string BankingRefNo { get; set; }
    public char TransactionSource { get; set; }
    public char CardDci { get; set; }
    public char CardBrand { get; set; }
    public char EntryType { get; set; }
    public ActionFlag TransactionActionFlags { get; set; }
    public TransferInfo TransferInformation { get; set; }
    public string QRData { get; set; }
    public int? SecurityLevelIndicator { get; set; }

}
