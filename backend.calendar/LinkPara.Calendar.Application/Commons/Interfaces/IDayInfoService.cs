namespace LinkPara.Calendar.Application.Commons.Interfaces;

public interface IDayInfoService
{
    Task<bool> IsHolidayAsync(string countryCode, DateTime date);

    Task<DateTime> GetPreviousWorkDayAsync(string countryCode, DateTime date);

    Task<DateTime> GetNextWorkDayAsync(string countryCode, DateTime date);
}
