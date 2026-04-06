using LinkPara.ApiGateway.CorporateWallet.Services.Location.Models.Responses.Countries;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Location.HttpClients;

public interface ICountryHttpClient
{
    Task<List<CountryDto>> GetCountriesAsync();

    Task<List<CityDto>> GetCitiesAsync(int countryCode);

    Task<List<DistrictDto>> GetDistrictsAsync(int cityCode);
}