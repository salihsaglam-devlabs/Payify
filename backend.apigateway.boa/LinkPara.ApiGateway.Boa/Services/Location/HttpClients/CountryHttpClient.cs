using LinkPara.ApiGateway.Boa.Services.Location.Models.Responses.Countries;
using System.Text.Json;

namespace LinkPara.ApiGateway.Boa.Services.Location.HttpClients;

public class CountryHttpClient : HttpClientBase, ICountryHttpClient
{
    public CountryHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
    }

    public async Task<List<CityDto>> GetCitiesAsync(int countryCode)
    {
        var response = await GetAsync($"v1/Countries/{countryCode}/Cities");
        var responseString = await response.Content.ReadAsStringAsync();

        var cities = JsonSerializer.Deserialize<List<CityDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return cities ?? throw new InvalidOperationException();
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

    public async Task<List<DistrictDto>> GetDistrictsAsync(int cityCode)
    {
        var response = await GetAsync($"v1/Countries/{cityCode}/Districts");
        var responseString = await response.Content.ReadAsStringAsync();

        var districts = JsonSerializer.Deserialize<List<DistrictDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return districts ?? throw new InvalidOperationException();
    }
}