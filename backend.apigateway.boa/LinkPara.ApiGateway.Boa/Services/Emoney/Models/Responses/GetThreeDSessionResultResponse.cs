namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;


public class GetThreeDSessionResultResponse : ResponseModel
{
    public string CurrentStep { get; set; }
    public string MdStatus { get; set; }
    public string MdErrorMessage { get; set; }
    public bool HalfSecure { get; set; } = false;
    public Guid CardTopupRequestId { get; set; }
}

