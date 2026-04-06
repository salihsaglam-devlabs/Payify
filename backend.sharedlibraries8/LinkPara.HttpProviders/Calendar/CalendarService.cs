using System.Globalization;
using System.Net.Http.Json;
using System.Web;
using LinkPara.Cache;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.Calendar;

public class CalendarService : HttpClientBase, ICalendarService
{
    private readonly ICacheService _cacheService;

    public CalendarService(HttpClient client, IHttpContextAccessor httpContextAccessor, ICacheService cacheService)
        : base(client, httpContextAccessor)
    {
        _cacheService = cacheService;
    }

    public async Task<bool> IsHolidayAsync(DateTime date, string countryCode)
    {
        var key = $"{countryCode}_IsHoliday_{date.ToShortDateString()}";

        return await _cacheService.GetOrCreateAsync(key, async () =>
        {
            var response = await GetAsync($"v1/days/isholiday?" +
                $"countryCode={countryCode}" +
                $"&date={HttpUtility.UrlEncode(date.ToString(CultureInfo.InvariantCulture))}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return Convert.ToBoolean(content);
            }
            else
            {
                throw new InvalidOperationException();
            }
        });
    }

    public async Task<DateTime> PreviousWorkDayAsync(DateTime date, string countryCode)
    {
        var key = $"{countryCode}_PreviousWorkDay_{date.ToShortDateString()}";

        return await _cacheService.GetOrCreateAsync(key, async () =>
        {
            var response =await GetAsync($"v1/days/previous-work-day?" +
                $"countryCode={countryCode}" +
                $"&date={HttpUtility.UrlEncode(date.ToString(CultureInfo.InvariantCulture))}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<DateTime>();
                return Convert.ToDateTime(content);
            }
            else
            {
                throw new InvalidOperationException();
            }
        });
    }

    public async Task<DateTime> NextWorkDayAsync(DateTime date, string countryCode)
    {
        var key = $"{countryCode}_NextWorkDay_{date.ToShortDateString()}";

        return await _cacheService.GetOrCreateAsync(key, async () =>
        {
            var response = await GetAsync($"v1/days/next-work-day" +
                $"?countryCode={countryCode}" +
                $"&date={HttpUtility.UrlEncode(date.ToString(CultureInfo.InvariantCulture))}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<DateTime>();
                return Convert.ToDateTime(content);
            }
            else
            {
                throw new InvalidOperationException();
            }
        });
    }
}