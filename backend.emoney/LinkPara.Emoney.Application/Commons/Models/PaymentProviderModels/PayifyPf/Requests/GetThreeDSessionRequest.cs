using LinkPara.Emoney.Application.Commons.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class GetThreeDSessionRequest : RequestHeaderInfo
{
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string CardToken { get; set; }
    public string Currency { get; set; }
    public PaymentType PaymentType { get; set; }
    public int InstallmentCount { get; set; }
    public string LanguageCode = "TR";
    public Guid CardTopupRequestId { get; set; }
}