using LinkPara.Calendar.Application.Commons.Interfaces;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Calendar.Infrastructure.Services;

public class DayInfoService : IDayInfoService
{
    private readonly IGenericRepository<HolidayDetail> _repository;

    public DayInfoService(IGenericRepository<HolidayDetail> repository)
    {
        _repository = repository;
    }

    public async Task<DateTime> GetPreviousWorkDayAsync(string countryCode, DateTime date)
    {
        date -= TimeSpan.FromDays(1);
        
        while (true)
        {
            var holiday = await IsHolidayAsync(countryCode, date);
            if (date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday || holiday)
            {
                date -= TimeSpan.FromDays(1);
            }
            else
            {
                break;
            }
        }

        return date;
    }

    public async Task<DateTime> GetNextWorkDayAsync(string countryCode, DateTime date)
    {
        date += TimeSpan.FromDays(1);

        while (true)
        {
            var holiday = await IsHolidayAsync(countryCode, date);
            if (date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday || holiday)
            {
                date += TimeSpan.FromDays(1);
            }
            else
            {
                break;
            }
        }

        return date;
    }

    public async Task<bool> IsHolidayAsync(string countryCode, DateTime date)
    {
        DateTime minDate, maxDate, beginTime;
        bool result = false;

        var details = await _repository.GetAll(s => s.Holiday)
                .Where(s => s.Holiday.CountryCode == countryCode)
                .ToListAsync();

        foreach (var item in details)
        {
            beginTime = item.BeginningTime;
            minDate = item.DateOfHoliday;
            maxDate = minDate.AddHours(item.DurationInDays * 24);

            if (date.CompareTo(beginTime) >= 0 && date.CompareTo(maxDate) < 0)
            {
                result = true;
                break;
            }
        }

        return result;
    }
}
