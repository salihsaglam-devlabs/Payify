namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

public class GetThreeDSessionResponse : ResponseModel
{
    public string ThreeDSessionId { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
