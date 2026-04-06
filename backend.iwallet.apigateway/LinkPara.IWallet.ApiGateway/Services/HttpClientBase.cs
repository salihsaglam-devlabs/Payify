using LinkPara.IWallet.ApiGateway.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;

namespace LinkPara.IWallet.ApiGateway.Services;


public class HttpClientBase
{
    private readonly HttpClient _client;

    protected HttpClientBase(HttpClient client, IHttpContextAccessor httpContextAccessor)
    {
        _client = client;

        var httpContext = httpContextAccessor.HttpContext;
        if(httpContext is not null)
        {
            var ipAddress = httpContext.Connection?.RemoteIpAddress?.ToString();

            if (ipAddress is not null)
            {
                _client.DefaultRequestHeaders.Add("ClientIpAddress", ipAddress);
            }
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
        baseUrl += '?';

        var type = fillWithAllParameters ? request.GetType() : typeof(SearchQueryParams);

        foreach (var property in type.GetProperties())
        {
            var value = property.GetValue(request);

            if (value is not null)
                baseUrl += $"{property.Name}={value}&";
        }

        return baseUrl[..^1];
    }

    protected async Task<BaseResponse> HandleExceptionAsync(HttpResponseMessage response) 
    {
        var baseResponse = new BaseResponse();
        var exceptionDetails = await response.Content.ReadAsStringAsync();
        var details = JsonConvert.DeserializeObject<ApiProblemDetail>(exceptionDetails);

        baseResponse.IsSuccess = false;
        baseResponse.ErrorCode = "999";
        baseResponse.Message = "InvalidOperation";

        if (details != null)
        {
            baseResponse.ErrorCode = !string.IsNullOrWhiteSpace(details.Code) ? details.Code : "999";
            baseResponse.Message = details.Detail;
        }

        return baseResponse;
    }
}