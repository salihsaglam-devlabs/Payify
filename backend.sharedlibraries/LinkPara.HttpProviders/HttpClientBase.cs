using LinkPara.SharedModels.Exceptions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Json;

namespace LinkPara.HttpProviders;

public class HttpClientBase
{
    private readonly HttpClient _client;

    public HttpClientBase(HttpClient client, IHttpContextAccessor httpContextAccessor)
    {
        _client = client;

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            ProcessBatchClient();
        }
        else
        {
            ProcessUserClient(httpContext);
        }
    }
    private void ProcessBatchClient()
    {
        _client.DefaultRequestHeaders.Add("Accept-Language", "en, tr-TR; q=0.9, tr; q=0.8, en-US; q=0.7");
        _client.DefaultRequestHeaders.Add("Channel", "Batch");
    }
    private void ProcessUserClient(HttpContext httpContext)
    {
        var userId = httpContext.Request?.Headers["UserId"];
        var ipAddress = httpContext.Request?.Headers["ClientIpAddress"];
        var port = httpContext.Request?.Headers["X-Forwarded-Port"];
        var userAgent = httpContext.Request?.Headers["User-Agent"];
        var language = httpContext.Request?.Headers["Accept-Language"];
        var channel = httpContext.Request?.Headers["Channel"];
        var authorization = httpContext.Request.Headers["Authorization"];
        var correlationId = httpContext.Request?.Headers["CorrelationId"];
        var approvalRequestId = httpContext.Request?.Headers["ApprovalRequestId"];

        if (userId is not null)
        {
            _client.DefaultRequestHeaders.Add("UserId", (string)userId);
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

        if (authorization.Count != 0)
        {
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization", (string)authorization);
        }

        if (correlationId is not null)
        {
            _client.DefaultRequestHeaders.Add("CorrelationId", (string)correlationId);
        }
        
        if (approvalRequestId is not null)
        {
            _client.DefaultRequestHeaders.Add("ApprovalRequestId", (string)approvalRequestId);
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

    public async Task<HttpResponseMessage> PatchAsync<T>(string requestUri, JsonPatchDocument<T> patchContent) where T : class
    {
        var response = new HttpResponseMessage();
        var options = new JsonSerializerOptions { WriteIndented = true };
        var addRequest = patchContent.Operations.Where(b => b.OperationType != OperationType.Replace);
        var replaceRequest = patchContent.Operations.Where(b => b.OperationType == OperationType.Replace);
        if (addRequest.Any())
        {
            string requestContent = JsonConvert.SerializeObject(addRequest);

            response = await PatchRequestAsync(requestUri, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                await HandleExceptionAsync(response);
            }
        }
        if (replaceRequest.Any())
        {
            string requestContent = System.Text.Json.JsonSerializer.Serialize(replaceRequest, options);

            response = await PatchRequestAsync(requestUri, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                await HandleExceptionAsync(response);
            }
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

    private async Task<HttpResponseMessage> PatchRequestAsync(string requestUri, string request)
    {
        ByteArrayContent content = new StringContent(request);

        content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);

        return await _client.PatchAsync(requestUri, content);
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
            }

            throw new UnauthorizedAccessException(message);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(response.RequestMessage?.RequestUri?.ToString());
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new Exception("BadHttpRequestException");
        }

        throw new Exception("InternalError");
    }
}