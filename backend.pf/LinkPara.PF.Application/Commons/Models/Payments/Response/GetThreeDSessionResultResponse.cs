namespace LinkPara.PF.Application.Commons.Models.Payments.Response;

public class GetThreeDSessionResultResponse : ResponseBase
{
    public string CurrentStep { get; set; }
    public string MdStatus { get; set; }
    public string MdErrorMessage { get; set; }
    public bool HalfSecure { get; set; }
}
