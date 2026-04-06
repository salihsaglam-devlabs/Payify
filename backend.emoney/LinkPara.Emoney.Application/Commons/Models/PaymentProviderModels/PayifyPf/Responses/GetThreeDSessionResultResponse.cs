namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class GetThreeDSessionResultResponse : ResponseModel
{
    public string CurrentStep { get; set; }
    public string MdStatus { get; set; }
    public string MdErrorMessage { get; set; }
    public bool HalfSecure { get; set; } = false;
    public Guid CardTopupRequestId { get; set; }
}