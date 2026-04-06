
namespace LinkPara.CampaignManagement.Application.Commons.Models.Responses;

public class TownResponse : BaseResponse
{
    public List<Town> Data { get; set; }
}

public class Town
{
    public int id { get; set; }
    public string name { get; set; }
}
