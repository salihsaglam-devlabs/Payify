using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class PricingProfileDto
{
    public decimal CommissionRate { get; set; }
    public decimal Fee { get; set; }
}
