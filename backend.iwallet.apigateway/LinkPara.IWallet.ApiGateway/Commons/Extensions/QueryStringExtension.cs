using System.Reflection;
using System.Web;

namespace LinkPara.IWallet.ApiGateway.Commons.Extensions;

public static class QueryStringExtension
{
    public static string GetQueryString(this object obj)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);

        foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties()
                     .Where(p => !p.GetIndexParameters().Any()))
        {
            var val = propertyInfo.GetValue(obj, null);

            if (val == null)
                continue;

            if (val.GetType() == typeof(DateTimeOffset))
            {
                val = $"{(DateTimeOffset)val:yyyy-MM-ddTHH:mm:ss.fffZ}";
            }

            query[propertyInfo.Name] = val.ToString();
        }

        if (!query.HasKeys())
            return "";

        return "?" + query;
    }
}
