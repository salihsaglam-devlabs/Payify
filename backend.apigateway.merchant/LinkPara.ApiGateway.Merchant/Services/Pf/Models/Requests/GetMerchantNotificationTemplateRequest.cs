using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetMerchantNotificationTemplateRequest
{
    public Guid Id { get; set; }
    public MerchantContentSource ContentSource { get; set; }
    public string Language { get; set; }
}