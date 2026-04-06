using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Threading.Channels;
using System.Web;

namespace LinkPara.PF.Pos.ApiGateway.Services;

public class HttpClientBase
{
    private readonly HttpClient _client;
    private readonly IApplicationUserService _applicationUserService;
    protected HttpClientBase(HttpClient client, IApplicationUserService applicationUserService, IHttpContextAccessor httpContextAccessor = null)
    {
        _client = client;

        string userId = null;
        string authorization = null;
        string gateway = null;
        string channel = null;
        string ipAddress = null;
        string ipAddressXForwarded = null;
        string port = null;
        string userAgent = null;
        string language = null;
        string correlationId = null;

        if (httpContextAccessor is not null)
        {
            var httpContext = httpContextAccessor?.HttpContext;

            userId = httpContext.Request?.Headers["UserId"];
            gateway = httpContext.Request?.Headers["Gateway"];
            channel = httpContext.Request?.Headers["Channel"];
            authorization = httpContext.Request?.Headers["Authorization"];
            userAgent = httpContext.Request?.Headers["User-Agent"];
            language = httpContext.Request?.Headers["Accept-Language"];
            ipAddress = httpContext.Request?.Headers["ClientIpAddress"];
            ipAddressXForwarded = httpContext.Request?.Headers["X-Forwarded-For"];
            port = httpContext.Request?.Headers["X-Forwarded-Port"];
            correlationId = httpContext.Request?.Headers["CorrelationId"];
        }

        if (userId is not null && authorization is not null)
        {
            _client.DefaultRequestHeaders.Add("Authorization", authorization);
            _client.DefaultRequestHeaders.Add("UserId", userId);

            if (!string.IsNullOrWhiteSpace(ipAddress))
            {
                _client.DefaultRequestHeaders.Add("ClientIpAddress", ipAddress);
            }
            else if (!string.IsNullOrWhiteSpace(ipAddressXForwarded))
            {
                _client.DefaultRequestHeaders.Add("ClientIpAddress", ipAddressXForwarded);
            }

            if (!string.IsNullOrWhiteSpace(ipAddressXForwarded))
            {
                _client.DefaultRequestHeaders.Add("X-Forwarded-For", ipAddressXForwarded);
            }

            if (port is not null)
            {
                _client.DefaultRequestHeaders.Add("X-Forwarded-Port", port);
            }

            if (userAgent is not null)
            {
                _client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            }

            if (language is not null)
            {
                _client.DefaultRequestHeaders.Add("Accept-Language", language);
            }

            if (channel is not null)
            {
                _client.DefaultRequestHeaders.Add("Channel", channel);
            }

            if(gateway is not null)
            {
                _client.DefaultRequestHeaders.Add("Gateway", gateway);
            }

            if (correlationId is null)
            {
                _client.DefaultRequestHeaders.Add("CorrelationId", Guid.NewGuid().ToString());
            }
            else
            {
                _client.DefaultRequestHeaders.Add("CorrelationId", correlationId);
            }
        }
        else
        {
            _client.DefaultRequestHeaders.Add("ClientIpAddress", ipAddressXForwarded);
            _applicationUserService = applicationUserService;
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_applicationUserService.Token}");
            _client.DefaultRequestHeaders.Add("Gateway", Gateway.PFPosGateway.ToString());
            _client.DefaultRequestHeaders.Add("CorrelationId", Guid.NewGuid().ToString());
        }
    }

    protected async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        var response = await _client.GetAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            await HandleExceptionAsync(response);
        }

        return response;
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        var response = await _client.PostAsJsonAsync(requestUri, value);

        if (!response.IsSuccessStatusCode)
        {
            await HandleExceptionAsync(response);
        }

        return response;
    }

    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
    {
        var response = await _client.PutAsync(requestUri, content);

        if (!response.IsSuccessStatusCode)
        {
            await HandleExceptionAsync(response);
        }

        return response;
    }

    public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value)
    {
        var response = await _client.PutAsJsonAsync(requestUri, value);

        if (!response.IsSuccessStatusCode)
        {
            await HandleExceptionAsync(response);
        }

        return response;
    }

    public async Task<HttpResponseMessage> PatchAsync(string requestUri, HttpContent content)
    {
        var response = await _client.PatchAsync(requestUri, content);

        if (!response.IsSuccessStatusCode)
        {
            await HandleExceptionAsync(response);
        }

        return response;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            await HandleExceptionAsync(response);
        }

        return response;
    }

    public async Task<HttpResponseMessage> DeleteAsync(string requestUri)
    {
        var response = await _client.DeleteAsync(requestUri);

        if (!response.IsSuccessStatusCode)
        {
            await HandleExceptionAsync(response);
        }

        return response;
    }

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
    public string CreateUrlWithParams<T>(string baseUrl, T request, bool fillWithAllParameters = false)
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
    public string CreateUrlWithProperties<T>(string baseUrl, T request)
    {
        return CreateParams(baseUrl, request, false);
    }

    private string CreateParams<T>(string baseUrl, T request, bool hasSearchQueryParams, bool fillWithAllParameters = false)
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

    private static async Task HandleExceptionAsync(HttpResponseMessage response)
    {
        var exceptionDetails = await response.Content.ReadAsStringAsync();
        var details = JsonConvert.DeserializeObject<ApiProblemDetail>(exceptionDetails);

        if (details != null)
        {
            if (details.Code == ErrorCode.ValidationError)
            {
                throw new ValidationException(details.Errors);
            }

            if (details.Code == ErrorCode.NotFound)
            {
                throw new NotFoundException(details.Detail);
            }

            if (details.Code == ErrorCode.DuplicateRecord)
            {
                throw new DuplicateRecordException(details.Detail);
            }

            if (response.StatusCode != HttpStatusCode.InternalServerError)
            {
                throw new ApiException(details.Code, details.Detail);
            }

            throw new Exception(details.Detail);
        }
        throw new Exception("InternalError");
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