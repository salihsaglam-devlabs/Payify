using LinkPara.HttpProviders.BusinessParameter.Models;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.IKS.Models.Response;
using LinkPara.HttpProviders.Location.Models;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Net.Http.Json;
using System.Text.Json;

namespace LinkPara.HttpProviders.Location;

public class LocationService : HttpClientBase, ILocationService
{

    public LocationService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task<CountryDto> GetCountryByCode(int countryCode)
    {
        var response = await GetAsync($"v1/Countries/{countryCode}");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var country = JsonSerializer.Deserialize<CountryDto>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return country;
        }
        throw new InvalidOperationException();
    }
    public async Task<List<CityDto>> GetCityByCode(int countryCode)
    {
        var response = await GetAsync($"v1/Countries/{countryCode}/cities");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

             var cities = JsonSerializer.Deserialize<List<CityDto>>(responseString,
             new JsonSerializerOptions
             {
                 PropertyNameCaseInsensitive = true
             });

            return cities;
        }
        throw new InvalidOperationException();
    }
    
    public async Task<CityDto> GetCityByCityCode(int cityCode)
    {
        var response = await GetAsync($"v1/Countries/city/{cityCode}");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var city = JsonSerializer.Deserialize<CityDto>(responseString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return city;
        }
        throw new InvalidOperationException();
    }
    
    public async Task<CityWithCountryDto> GetCountryByCityCode(int cityCode)
    {
        var response = await GetAsync($"v1/Countries/{cityCode}/country");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var cityWithCountry = JsonSerializer.Deserialize<CityWithCountryDto>(responseString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return cityWithCountry;
        }
        throw new InvalidOperationException();
    }

    public async Task<List<DistrictDto>> GetDistrictsByCityCode(int cityCode)
    {
        var response = await GetAsync($"v1/Countries/{cityCode}/districts");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var districts = JsonSerializer.Deserialize<List<DistrictDto>>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return districts;
        }
        throw new InvalidOperationException();
    }

    public async Task<CityDto> GetCityByIso2Code(int countryCode, string cityIso2Code)
    {
        var response = await GetAsync($"v1/Countries/{countryCode}/city/{cityIso2Code}");

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            var city = JsonSerializer.Deserialize<CityDto>(responseString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return city;
        }
        throw new InvalidOperationException();
    }

    public async Task UpdateDistrictAsync(UpdateDistrictRequest request)
    {
        var response = await PutAsJsonAsync("v1/Countries/UpdateDistrict", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }
}