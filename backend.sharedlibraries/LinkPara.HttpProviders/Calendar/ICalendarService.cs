namespace LinkPara.HttpProviders.Calendar;

public interface ICalendarService
{
    Task<bool> IsHolidayAsync(DateTime date, string countryCode);
    Task<DateTime> PreviousWorkDayAsync(DateTime date, string countryCode);
    Task<DateTime> NextWorkDayAsync(DateTime date, string countryCode);
}
