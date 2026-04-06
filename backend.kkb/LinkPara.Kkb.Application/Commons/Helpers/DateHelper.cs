namespace LinkPara.Kkb.Application.Commons.Helpers;

public static class DateHelper
{
    private const string TimeZoneName = "Turkey Standard Time";

    public static DateTimeOffset GetLocalDate()
    {
        var dtoUtc = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);

        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneName);

        var dtoLocal = TimeZoneInfo.ConvertTime(dtoUtc, localTimeZone);

        return dtoLocal;
    }
}
