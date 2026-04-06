namespace LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

public class CampaignResponse
{
    public int Id { get; set; }
    public string CampaignType { get; set; }
    public decimal MinAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string CbType { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public string ImageUrl { get; set; }
    public List<CampaignMerchantResponse> CampaignMerchants { get; set; }
}
