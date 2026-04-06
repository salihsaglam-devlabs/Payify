using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class HostedPaymentInstallmentDto
{
    public CardNetwork CardNetwork { get; set; }
    public int Installment { get; set; }
    public decimal? Amount { get; set; }
}