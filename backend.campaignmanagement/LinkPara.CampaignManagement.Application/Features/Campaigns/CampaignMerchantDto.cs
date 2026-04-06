namespace LinkPara.CampaignManagement.Application.Features.Campaigns;

public class CampaignMerchantDto
{
    public decimal CbAmount { get; set; }
    public string Content { get; set; }
    public string ImageUrl { get; set; }
    public MerchantDto Merchant { get; set; }
    public string Description { get; set; }
}
