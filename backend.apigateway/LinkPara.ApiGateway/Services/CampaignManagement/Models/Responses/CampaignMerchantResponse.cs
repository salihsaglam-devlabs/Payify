namespace LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

public class CampaignMerchantResponse
{
    public decimal CbAmount { get; set; }
    public string Content { get; set; }
    public string ImageUrl { get; set; }
    public MerchantResponse Merchant { get; set; }
    public string Description { get; set; }
}
