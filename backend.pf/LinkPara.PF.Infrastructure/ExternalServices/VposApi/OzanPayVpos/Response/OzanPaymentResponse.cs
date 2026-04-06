namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Response;

public class FraudDetail
{
    public string FraudResult { get; set; }
}

public class OzanPaymentResponse : OzanPayResponseBase
{
    public bool Is3D { get; set; }
    public bool Only3D { get; set; }
    public string ReturnUrl { get; set; }
    public long DateEpoch { get; set; }
    public bool IsLive { get; set; }
    public string Descriptor { get; set; }
    public FraudDetail FraudDetail { get; set; }
    public string Form3D { get; set; }
    public string CustomerIp { get; set; }
    public string CustomerUserAgent { get; set; }
    public int Amount { get; set; }
    public string Currency { get; set; }
    public int AmountProcessed { get; set; }
    public int InstallmentCount { get; set; }
}
