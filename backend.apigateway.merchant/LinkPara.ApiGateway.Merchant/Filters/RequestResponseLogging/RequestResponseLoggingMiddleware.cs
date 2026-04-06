using System.Diagnostics;
using System.Text.Json;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Microsoft.IO;

namespace LinkPara.ApiGateway.Merchant.Filters.RequestResponseLogging;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly RequestResponseLoggingSettings _requestResponseLoggingSettings;
    private readonly IBus _eventBus;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private const int MaxBodyLogSize = 10_000; 
    bool requestIsSensitive = true;
    bool responseIsSensitive = true;

    public RequestResponseLoggingMiddleware(
        IConfiguration configuration,
        RequestDelegate next,
        IBus eventBus,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _eventBus = eventBus;
        _next = next;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        _logger = logger;

        _requestResponseLoggingSettings = new RequestResponseLoggingSettings();
        configuration.GetSection(nameof(RequestResponseLoggingSettings)).Bind(_requestResponseLoggingSettings);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_requestResponseLoggingSettings.IsEnabled)
        {
            SetSensitiveData(context);

            // request
            context.Request.EnableBuffering();
            using var requestStream = _recyclableMemoryStreamManager.GetStream();
            await context.Request.Body.CopyToAsync(requestStream);
            context.Request.Body.Position = 0;
            var requestString = await ReadStreamLimited(context.Request.Body);
            context.Request.Body.Position = 0;
            var request = InitRequestLog(context, requestString);

            //response
            var originalBodyStream = context.Response.Body;
            using var responseBody = _recyclableMemoryStreamManager.GetStream();
            context.Response.Body = responseBody;

            var watch = Stopwatch.StartNew();
            await _next(context);
            watch.Stop();

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseString = await ReadStreamLimited(context.Response.Body);
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            var response = InitResponseLog(context, responseString, watch.ElapsedMilliseconds);

            // publish to rabbitmq

            var requestResponseLog = new RequestResponseLogCreated
            {
                CreatedDate = DateTime.Now,
                Request = request,
                Response = response
            };
            _ = Task.Run(async () => await PublishQueueAsync(requestResponseLog));

            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task PublishQueueAsync(RequestResponseLogCreated requestResponseLog)
    {
        try
        {
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _eventBus.Publish(requestResponseLog, token.Token);
            token.Dispose();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "RequestResponseLoggingMiddleware error");
        }
    }
    private async Task<string> ReadStreamLimited(Stream stream)
    {
        stream.Position = 0;

        using var reader = new StreamReader(stream, leaveOpen: true);

        char[] buffer = new char[MaxBodyLogSize];
        int read = await reader.ReadBlockAsync(buffer, 0, MaxBodyLogSize);

        stream.Position = 0;

        return new string(buffer, 0, read);
    }
    private ApiRequest InitRequestLog(HttpContext context, string request)
    {
        var paramQueryString = !requestIsSensitive ? context.Request.QueryString.Value : "sensitive!";
        var paramBody = !requestIsSensitive ? request : "sensitive!";

        var header = GetSerializedHeader(context);

        return new ApiRequest
        {
            Host = context.Request.Host.Value,
            IPAdress = context.Connection.RemoteIpAddress.ToString(),
            Method = context.Request.Method,
            Path = context.Request.Path,
            Querystring = paramQueryString,
            RequestBody = paramBody,
            Scheme = context.Request.Scheme,
            Header = header,
        };
    }
    private static string GetSerializedHeader(HttpContext context)
    {
        var arrLenght = context.Request.Headers.Count;
        KeyValuePair<string, StringValues>[] headers = new KeyValuePair<string, StringValues>[arrLenght];
        context.Request.Headers.CopyTo(headers, 0);

        var headerList = headers.ToList();
        if (headerList.Any(x => x.Key == "Authorization"))
        {
            headerList.RemoveAll(x => x.Key == "Authorization");
        }

        return JsonSerializer.Serialize(headerList);
    }
    private ApiResponse InitResponseLog(HttpContext context, string response, long elapsedMiliseconds)
    {
        var paramResponse = !responseIsSensitive ? response : "sensitive!";

        return new ApiResponse
        {
            ElapsedTime = elapsedMiliseconds,
            Response = paramResponse,
            StatusCode = context.Response.StatusCode,
        };
    }

    private void SetSensitiveData(HttpContext context)
    {
        SensitiveDataType? sensitiveData = null;

        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var attribute = endpoint?.Metadata.GetMetadata<SensitiveDataAttribute>();
        if (attribute is not null)
        {
            sensitiveData = attribute.SensitiveData;
        }

        requestIsSensitive = sensitiveData != null && (sensitiveData.Value == SensitiveDataType.All || sensitiveData.Value == SensitiveDataType.Request);
        responseIsSensitive = sensitiveData != null && (sensitiveData.Value == SensitiveDataType.All || sensitiveData.Value == SensitiveDataType.Response);
    }
}