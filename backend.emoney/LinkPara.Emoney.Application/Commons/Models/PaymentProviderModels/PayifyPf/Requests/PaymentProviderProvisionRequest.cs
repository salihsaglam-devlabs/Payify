using LinkPara.Emoney.Application.Commons.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class PaymentProviderProvisionRequest : RequestHeaderInfo
{
    public decimal Amount { get; set; }
    public string CardToken { get; set; }
    public string Currency { get; set; }
    public PaymentType PaymentType { get; set; }
    public string ThreeDSessionId { get; set; }
    public string CardHolderName { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
