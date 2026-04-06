namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request;

public class PosnetPaymentBase : PosnetRequestBase
{
    public string Amount { get; set; }
    public string CardNo { get; set; }
    public string CurrencyCode { get; set; }
    public string CardHolderName { get; set; }
    public string Cvv { get; set; }
    public string ExpireDate { get; set; }
    public int NumberOfInstallments { get; set; }
    public string MailOrderFlag { get; set; }
    public int KoiCode { get; set; }
    public string SubMerchantId { get; set; }
    public string MrcPfId { get; set; }
    public string Mcc { get; set; }
    public string Tckn { get; set; }
    public string Vkn { get; set; }
    public string SubDealerCode { get; set; }
    public string TransactionType { get; set; }
    public string LanguageCode { get; set; }
    public string SuccessUrl { get; set; }
    public string FailureUrl { get; set; }
}