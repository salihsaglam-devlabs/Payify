using System;

namespace Microsoft.Extensions.Localization;

public static class StringLocalizerExtensions
{
    public static string Get(this IStringLocalizer localizer, string key)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        return localizer[key].Value;
    }

    public static string Get(this IStringLocalizer localizer, string key, params object[] arguments)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        return localizer[key, arguments].Value;
    }
}
