namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
public class FinansPaymentBase : FinansRequestBase
{
    public string OrderId { get; set; }
    public string SecureType { get; set; }
    public string PurchAmount { get; set; }
    public string BonusAmount { get; set; }
    public string TxnType { get; set; }
    public int? InstallmentCount { get; set; }
    public int CurrencyCode { get; set; }
    public string Rnd { get; set; }
    public string Pan { get; set; }
    public string Cvv2 { get; set; }
    public string MOTO { get; set; }
    public string Expiry { get; set; }
    public string HashAlgorithm { get; set; }
    public string Hash { get; set; }
    public string CardHolderName { get; set; }
    public string PaymentFacilitatorId { get; set; }
    public string SubmerchantId { get; set; }
    public string SubmerchantMcc { get; set; }
    public string CardAcceptorName { get; set; }
    public string CardAcceptorStreet { get; set; }
    public string CardAcceptorCity { get; set; }
    public string CardAcceptorPostalCode { get; set; }
    public string CardAcceptorState { get; set; }
    public string CardAcceptorCountry { get; set; }
}