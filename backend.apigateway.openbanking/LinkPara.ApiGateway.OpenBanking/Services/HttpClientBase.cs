using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Web;

namespace LinkPara.ApiGateway.OpenBanking.Services;

public class HttpClientBase
{
    private readonly HttpClient _client;
    public readonly Guid AccountId;

    protected HttpClientBase(HttpClient client, IHttpContextAccessor httpContextAccessor)
    {
        _client = client;

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            var userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null)
            {
                _client.DefaultRequestHeaders.Add("UserId", userId);
            }

            var ipAddress = httpContext.Request?.Headers["X-Forwarded-For"];
            var port = httpContext.Request?.Headers["X-Forwarded-Port"];

            var userAgent = httpContext.Request?.Headers["User-Agent"];
            var language = httpContext.Request?.Headers["Accept-Language"];
            var channel = httpContext.Request?.Headers["Channel"];

            var customerId = httpContext.Request?.Headers["x-customer-id"];

            if (!string.IsNullOrWhiteSpace(customerId))
            {
                if (!Guid.TryParse(customerId, out var accountId))
                {
                    throw new InvalidCastException();
                }

                AccountId = accountId;
            }

            if (ipAddress is not null)
            {
                _client.DefaultRequestHeaders.Add("ClientIpAddress", (string)ipAddress);
                _client.DefaultRequestHeaders.Add("X-Forwarded-For", (string)ipAddress);
            }
            if (port is not null)
            {
                _client.DefaultRequestHeaders.Add("X-Forwarded-Port", (string)port);
            }
            if (userAgent is not null)
            {
                _client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
            }
            if (language is not null)
            {
                _client.DefaultRequestHeaders.Add("Accept-Language", (string)language);
            }
            if (channel is not null)
            {
                _client.DefaultRequestHeaders.Add("Channel", (string)channel);
            }
        }

        //ToDo : Add Gateway
        //_client.DefaultRequestHeaders.Add("Gateway", Gateway.OpenBanking.ToString()); 
        _client.DefaultRequestHeaders.Add("CorrelationId", Guid.NewGuid().ToString());
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

    protected async Task<HttpResponseMessage> PostAsync(string requestUri, string jsonValue)
    {
        var content = new StringContent(jsonValue, Encoding.UTF8, "application/json");
        var response = _client.PostAsync(requestUri, content).Result;

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
    public async Task<HttpResponseMessage> PatchAsync<T>(string requestUri, JsonPatchDocument<T> patchContent) where T : class
    {
        var response = new HttpResponseMessage();
        var options = new JsonSerializerOptions { WriteIndented = true };
        var addRequest = patchContent.Operations.Where(b => b.OperationType != OperationType.Replace);
        var replaceRequest = patchContent.Operations.Where(b => b.OperationType == OperationType.Replace);
        if (addRequest.Any())
        {
            var requestContent = JsonConvert.SerializeObject(addRequest);

            response = await PatchRequestAsync(requestUri, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                await HandleExceptionAsync(response);
            }
        }
        if (replaceRequest.Any())
        {
            var requestContent = System.Text.Json.JsonSerializer.Serialize(replaceRequest, options);

            response = await PatchRequestAsync(requestUri, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                await HandleExceptionAsync(response);
            }
        }

        return response;
    }

    public async Task<HttpResponseMessage> PatchAsJsonAsync(string requestUri, string jsonContent)
    {
        ByteArrayContent content = new StringContent(jsonContent);

        content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

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

    protected static async Task HandleExceptionAsync(HttpResponseMessage response)
    {
        if (response.Content.Headers.ContentType is not null)
        {
            var hasProblemDetail = response.Content.Headers.ContentType.MediaType ?? string.Empty;

            if (hasProblemDetail == "application/problem+json")
            {
                await HandleExceptionWithProblemDetailAsync(response);
                return;
            }
        }

        HandleStandardException(response);
    }

    private static async Task HandleExceptionWithProblemDetailAsync(HttpResponseMessage response)
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

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(details.Code);
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new ForbiddenAccessException();
            }

            if (response.StatusCode != HttpStatusCode.InternalServerError)
                throw new ApiException(details.Code, details.Detail);

            throw new Exception(details.Detail);
        }
        throw new Exception("InternalError");
    }

    private static async void HandleStandardException(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var message = string.Empty;

            try
            {
                message = await response.Content?.ReadAsStringAsync();
            }
            catch
            {
                // will be removed
            }

            throw new UnauthorizedAccessException(message);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(response.RequestMessage?.RequestUri?.ToString());
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadHttpRequestException("BadRequest");
        }

        throw new Exception("InternalError");
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

    private async Task<HttpResponseMessage> PatchRequestAsync(string requestUri, string request)
    {
        ByteArrayContent content = new StringContent(request);

        content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

        return await _client.PatchAsync(requestUri, content);
    }
}