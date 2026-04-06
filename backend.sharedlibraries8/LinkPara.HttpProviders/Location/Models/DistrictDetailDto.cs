namespace LinkPara.HttpProviders.Location.Models;

public class DistrictDetailDto
{
    public int DistrictCode { get; set; }
    public string Name { get; set; }
    public int CityCode { get; set; }
    public string CityName { get; set; }
    public int CountryCode { get; set; }
    public string CountryName { get; set; }
}