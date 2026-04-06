using LinkPara.SharedModels.Pagination;
using System.Globalization;
using System.Web;

namespace LinkPara.HttpProviders.Utility;

public static class GetQueryString
{

    /// <summary>
    /// Adds query parameters to the baseUrl. 
    /// <br/><br/>
    /// By default, only SearchQueryParams(Q,Page,Size) parameters will be added to the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="baseUrl">v1/transactions</param>
    /// <param name="request"></param>
    /// <param name="fillWithAllParameters">Pass this true if you want to see all properties of request in query.</param>
    /// <returns>v1/transactions?Q=query&#38;Size=20&#38;Page=1</returns>
    public static string CreateUrlWithSearchQueryParams<T>(string baseUrl, T request, bool fillWithAllParameters = false)
        where T : SearchQueryParams
    {
        return CreateParams(baseUrl, request, true, fillWithAllParameters);
    }

    /// <summary>
    /// It adds all fields of an object as a query parameter.
    /// </summary>
    /// <param name="baseUrl">ex: v1/transactions</param>
    /// <param name="request">object instance</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string CreateUrlWithParams<T>(string baseUrl, T request)
    {
        return CreateParams(baseUrl, request, false);
    }

    private static string CreateParams<T>(string baseUrl, T request, bool hasSearchQueryParams, bool fillWithAllParameters = false)
    {
        baseUrl += '?';

        var type = hasSearchQueryParams
            ? fillWithAllParameters
                ? request.GetType()
                : typeof(SearchQueryParams)
            : request.GetType();

        foreach (var property in type.GetProperties())
        {
            var value = property.GetValue(request);

            if (value is not null)
            {
                if (value.GetType() == typeof(DateTime) || value.GetType() == typeof(DateTimeOffset))
                {
                    baseUrl += $"{property.Name}={HttpUtility.UrlEncode($"{(DateTime)value:yyyy-MM-ddTHH:mm:ss}")}&";
                }
                else if (value.GetType().IsArray)
                {
                    foreach (var item in (Array)value)
                    {
                        baseUrl += $"{property.Name}={HttpUtility.UrlEncode(ConvertIfValueIsFractional(item))}&";
                    }
                }
                else
                {
                    baseUrl += $"{property.Name}={HttpUtility.UrlEncode(ConvertIfValueIsFractional(value))}&";
                }

            }
        }

        return baseUrl[..^1];
    }
    
    private static string ConvertIfValueIsFractional(object value)
    {
        if (value is decimal or float or double)
        {
            var isParsed = double.TryParse(value.ToString(), out var numericValue);
            
            if (isParsed)
            {
                return $"{numericValue.ToString(CultureInfo.InvariantCulture)}";
            }
        }
        
        return value.ToString();
    }
}
