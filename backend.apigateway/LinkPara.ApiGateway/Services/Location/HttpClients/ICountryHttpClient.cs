using LinkPara.ApiGateway.Services.Location.Models.Responses.Countries;

namespace LinkPara.ApiGateway.Services.Location.HttpClients;

public interface ICountryHttpClient
{
    Task<List<CountryDto>> GetCountriesAsync();

    Task<List<CityDto>> GetCitiesAsync(int countryId);

    Task<List<DistrictDto>> GetDistrictsAsync(int cityId);
}
