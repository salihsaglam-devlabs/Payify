using LinkPara.HttpProviders.Fraud.Models.Enums;

namespace LinkPara.HttpProviders.Fraud.Models;

public class FraudTransactionDetail
{
    public decimal Amount { get; set; }
    public int AmountCurrencyCode { get; set; }
    public string Beneficiary { get; set; }
    public int BeneficiaryAccountCurrencyCode { get; set; }
    public string BeneficiaryNumber { get; set; }
    public string BeneficiaryBankID { get; set; }
    public int OriginatorAccountCurrencyCode { get; set; }
    public string OriginatorNumber { get; set; }
    public string Originator { get; set; }
    public string OriginatorBankID { get; set; }
    public FraudSource FraudSource { get; set; }
    public Direction Direction { get; set; }
    public string Channel { get; set; }
    public int? MccCode { get; set; }
    public string TransactionType { get; set; }
    public Actions? Action { get; set; }
    public Response? Response{ get; set; }
}
