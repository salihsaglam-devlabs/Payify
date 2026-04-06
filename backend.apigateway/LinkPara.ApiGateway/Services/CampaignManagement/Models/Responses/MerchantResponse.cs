namespace LinkPara.ApiGateway.Services.CampaignManagement.Models.Responses;

public class MerchantResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public List<string> SectorList { get; set; }
    public string Logo { get; set; }
    public List<SectorResponse> Sectors { get; set; }
}
