
namespace LinkPara.CampaignManagement.Application.Features.Campaigns;

public class MerchantDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public List<string> SectorList { get; set; }
    public string Logo { get; set; }
    public List<SectorDto> Sectors { get; set; }
}
