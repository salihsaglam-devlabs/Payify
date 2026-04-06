namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class Verify3dsResponse : ResponseModel
{
    public string CallbackUrl { get; set; }
    public string MdStatus { get; set; }
    public string ThreeDSessionId { get; set; }
    public bool HalfSecure { get; set; }
}
