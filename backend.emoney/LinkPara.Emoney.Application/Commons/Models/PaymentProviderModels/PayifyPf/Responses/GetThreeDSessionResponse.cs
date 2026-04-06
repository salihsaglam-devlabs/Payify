namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class GetThreeDSessionResponse : ResponseModel
{
    public string ThreeDSessionId { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
