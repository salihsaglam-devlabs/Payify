using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetThreeDSessionRequest
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
