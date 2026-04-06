using LinkPara.ApiGateway.Merchant.Services.Location.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Location.Models.Responses.Countries;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Merchant.Controllers.Location;

public class CountriesController : ApiControllerBase
{
    private readonly ICountryHttpClient _countryHttpClient;

    public CountriesController(ICountryHttpClient countryHttpClient)
    {
        _countryHttpClient = countryHttpClient;
    }

    /// <summary>
    /// Returns all countries.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public async Task<ActionResult<List<CountryDto>>> GetCountriesAsync()
    {
        return await _countryHttpClient.GetCountriesAsync();
    }

    /// <summary>
    /// Returns all the cities in the country which is filtered by country Code.
    /// </summary>
    /// <param name="countryCode"></param>
    /// <returns></returns>
    [HttpGet("{countryCode}/cities")]
    public async Task<ActionResult<List<CityDto>>> GetCitiesAsync([FromRoute] int countryCode)
    {
        return await _countryHttpClient.GetCitiesAsync(countryCode);
    }

    /// <summary>
    /// Returns all the districts in the city which is filtered by city code.
    /// </summary>
    /// <param name="cityCode"></param>
    /// <returns></returns>
    [HttpGet("{cityCode}/districts")]
    public async Task<ActionResult<List<DistrictDto>>> GetDistrictsAsync([FromRoute] int cityCode)
    {
        return await _countryHttpClient.GetDistrictsAsync(cityCode);
    }
}