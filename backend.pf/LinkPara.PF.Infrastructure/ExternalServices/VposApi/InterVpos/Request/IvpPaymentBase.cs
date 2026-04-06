namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Request;

public class IvpPaymentBase : IvpRequestBase
{
    public string OrderId { get; set; }
    public string PurcAmount { get; set; }
    public int Currency { get; set; }
    public int InstallmentCount { get; set; }
    public string SubMerchantCode { get; set; }
    public string Pan { get; set; }
    public string Cvv2 { get; set; }
    public string Expiry { get; set; }
    public decimal BonusAmount { get; set; }
}
