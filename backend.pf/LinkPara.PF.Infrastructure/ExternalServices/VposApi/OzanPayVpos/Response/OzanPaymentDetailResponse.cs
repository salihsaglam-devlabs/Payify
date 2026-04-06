namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Response;

public class OzanPaymentDetailResponse : OzanPayResponseBase
{
    public int Amount { get; set; }
    public string Currency { get; set; }
    public string CardNumber { get; set; }
    public string Created_At { get; set; }
    public string Updated_At { get; set; }
}
