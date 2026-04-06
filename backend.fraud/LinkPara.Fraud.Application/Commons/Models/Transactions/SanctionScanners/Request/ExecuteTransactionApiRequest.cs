namespace LinkPara.Fraud.Application.Commons.Models.Transactions.SanctionScanners.Request;

public class ExecuteTransactionApiRequest
{
    public decimal Amount { get; set; }
    public int AmountCurrencyCode { get; set; }
    public string TransactionIPAddress { get; set; }
    public string Beneficiary { get; set; }
    public int BeneficiaryAccountCurrencyCode { get; set; }
    public string BeneficiaryBankID { get; set; }
    public string BeneficiaryNumber { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Narrative { get; set; }
    public string Originator { get; set; }
    public int OriginatorAccountCurrencyCode { get; set; }
    public int Originator​Type​Id { get; set; }
    public string OriginatorBankID { get; set; }
    public string OriginatorNumber { get; set; }
    public int MccCode { get; set; }
    //
    // Summary:
    //     1 : EFT, 2 : Remittance, 3 : Swift, 4 : ATM, 5 : POS, 6 : MoneyTransfer, 7 :
    //     Wallet
    public int Source { get; set; }
    //
    // Summary:
    //     1 : Inbound 2 : Outbound
    public int Direction { get; set; }
    //
    // Summary:
    //     1 : Internet 2 : Mobile 3 : ATM
    public int Channel { get; set; }
    public string TransactionID { get; set; }
    public string TriggeredRuleSetKey { get; set; }
}
