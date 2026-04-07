namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsers;

internal static class FixedWidthExtensions
{
    public static string SliceOrEmpty(this string value, int start, int length)
    {
        if (string.IsNullOrEmpty(value) || start >= value.Length || length <= 0)
        {
            return string.Empty;
        }

        var safeLength = Math.Min(length, value.Length - start);
        return value.Substring(start, safeLength).Trim();
    }
}
