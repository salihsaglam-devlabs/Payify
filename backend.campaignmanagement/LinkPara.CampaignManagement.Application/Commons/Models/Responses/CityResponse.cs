
namespace LinkPara.CampaignManagement.Application.Commons.Models.Responses;

public class CityResponse : BaseResponse
{
    public List<City> Data { get; set; }
}

public class City
{
    public int id { get; set; }
    public string name { get; set; }
}
