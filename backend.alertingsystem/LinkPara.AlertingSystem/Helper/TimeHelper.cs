namespace LinkPara.AlertingSystem.Helper;

public class TimeHelper
{
    public static DateTime ConvertTimeStampToDateTime(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
    }
}