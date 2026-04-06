using LinkPara.ApiGateway.Services.Location.Models.Responses.Countries;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.Services.Location.HttpClients;

public class CountryHttpClient : HttpClientBase, ICountryHttpClient
{
    public CountryHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<List<CountryDto>> GetCountriesAsync()
    {
        var response = await GetAsync($"v1/Countries");
        var responseString = await response.Content.ReadAsStringAsync();

        var countries = JsonSerializer.Deserialize<List<CountryDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return countries ?? throw new InvalidOperationException();
    }

    public async Task<List<CityDto>> GetCitiesAsync(int countryId)
    {
        var response = await GetAsync($"v1/Countries/{countryId}/Cities");
        var responseString = await response.Content.ReadAsStringAsync();

        var cities = JsonSerializer.Deserialize<List<CityDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return cities ?? throw new InvalidOperationException();
    }

    public async Task<List<DistrictDto>> GetDistrictsAsync(int cityId)
    {
        var response = await GetAsync($"v1/Countries/{cityId}/Districts");
        var responseString = await response.Content.ReadAsStringAsync();

        var districts = JsonSerializer.Deserialize<List<DistrictDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return districts ?? throw new InvalidOperationException();
    }
}