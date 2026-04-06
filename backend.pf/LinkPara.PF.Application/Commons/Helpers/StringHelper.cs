namespace LinkPara.PF.Application.Commons.Helpers;

public static class StringHelper
{
    public static string Truncate(this string value, int maxLength)
    {
        return value?.Length > maxLength ? value[..maxLength] : value;
    }
}