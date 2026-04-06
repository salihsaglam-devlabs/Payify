namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class GetThreeDSessionResultResponse : ResponseModel
{
    public string CurrentStep { get; set; }
    public string MdStatus { get; set; }
    public string MdErrorMessage { get; set; }
    public bool HalfSecure { get; set; }
}
