namespace LinkPara.HttpProviders.Location.Models
{
    public class UpdateDistrictRequest 
    {
        public int CountryCode { get; set; }
        public int CityCode { get; set; }
        public int DistrictCode { get; set; }
        public string KpsDistrictCode { get; set; }
    }
}
