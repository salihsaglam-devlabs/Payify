namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;

public class AkbankPaymentBase : AkbankRequestBase
{
    public string CardNumber { get; set; }
    public string Cvv2 { get; set; }
    public string ExpireDate { get; set; }
    public string MotoInd { get; set; }
    public int InstallCount { get; set; }
    public string OkUrl { get; set; }
    public string FailUrl { get; set; }
    public string PaymentModel { get; set; }
    public string PostUrl { get; set; }
    public string Amount { get; set; }
    public string BonusAmount { get; set; }

}
