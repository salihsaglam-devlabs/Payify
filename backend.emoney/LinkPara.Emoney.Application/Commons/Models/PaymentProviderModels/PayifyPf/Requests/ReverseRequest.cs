using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class ReverseRequest : RequestHeaderInfo
{
    public string OrderId { get; set; }
    public string LanguageCode = "TR";
    public SignatureDataResponse SignatureDataResponse { get; set; }
}