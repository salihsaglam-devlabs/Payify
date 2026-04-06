using LinkPara.ApiGateway.Services.Emoney.Models.Requests;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;
public class GetFuturePaymentOrderListResponseDto : GetFuturePaymentOrderListResultDto
{
    public string RizaNo { get; set; }
    public string RizaDrm { get; set; }
    public string RizaIptDtyKod { get; set; }
    public ProcessAmountOb IslTtr { get; set; }
    public ProcessAmountOb GrckIslTtr { get; set; }
    public RetailPersonOb Gon { get; set; }
    public RetailPersonOb Alc { get; set; }
}



