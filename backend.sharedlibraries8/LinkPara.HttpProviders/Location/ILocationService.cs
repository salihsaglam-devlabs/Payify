using LinkPara.HttpProviders.IKS.Models.Request;
using LinkPara.HttpProviders.IKS.Models.Response;
using LinkPara.HttpProviders.Location.Models;

namespace LinkPara.HttpProviders.Location;

public interface ILocationService
{
    Task<CountryDto> GetCountryByCode(int countryCode);
    Task <List<CityDto>> GetCityByCode(int countryCode);
    Task<CityDto> GetCityByCityCode(int cityCode);
    Task<CityWithCountryDto> GetCountryByCityCode(int cityCode);
    Task<List<DistrictDto>> GetDistrictsByCityCode(int cityCode);
    Task<CityDto> GetCityByIso2Code(int countryCode, string cityIso2Code);
    Task UpdateDistrictAsync(UpdateDistrictRequest request);
    Task<List<DistrictDetailDto>> GetDistrictsByDistrictCodes(List<int> districtCodes);

}
