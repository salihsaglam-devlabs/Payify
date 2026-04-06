using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class ReturnRequest : RequestHeaderInfo
{
    public string OrderId { get; set; }
    public string LanguageCode { get; set; } = "TR";
    public decimal Amount { get; set; }
    public SignatureDataResponse SignatureDataResponse { get; set; }
}
