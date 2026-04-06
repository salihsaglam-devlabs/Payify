using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;
using Microsoft.Extensions.Primitives;
using Microsoft.IO;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace LinkPara.PF.Pos.ApiGateway.Filters.RequestResponseLogging;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly RequestResponseLoggingSettings _requestResponseLoggingSettings;
    private readonly IBus _eventBus;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        IConfiguration configuration,
        RequestDelegate next,
        IBus eventBus,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
        _next = next;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

        _requestResponseLoggingSettings = new RequestResponseLoggingSettings();
        configuration.GetSection(nameof(RequestResponseLoggingSettings)).Bind(_requestResponseLoggingSettings);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_requestResponseLoggingSettings.IsEnabled)
        {
            await _next(context);
            return;
        }
        // request
        context.Request.EnableBuffering();
        using var requestStream = _recyclableMemoryStreamManager.GetStream();
        await context.Request.Body.CopyToAsync(requestStream);
        context.Request.Body.Position = 0;
        var requestString = await new StreamReader(context.Request.Body).ReadToEndAsync();
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
        var responseString = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        var response = InitResponseLog(context, responseString, watch.ElapsedMilliseconds);

        var requestResponseLog = new RequestResponseLogCreated
        {
            CreatedDate = DateTime.UtcNow,
            Request = request,
            Response = response
        };
        _ = Task.Run(async () => await PublishQueueAsync(requestResponseLog));

        await responseBody.CopyToAsync(originalBodyStream);

    }

    private async Task PublishQueueAsync(RequestResponseLogCreated requestResponseLog)
    {
        try
        {
            CancellationTokenSource token = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _eventBus.Publish<RequestResponseLogCreated>(requestResponseLog, token.Token);
            token.Dispose();
        }
        catch (Exception exception)
        {
            _logger.LogError($"RequestResponseLoggingMiddleware detail: \n{exception}");
            throw;
        }
    }

    private ApiRequest InitRequestLog(HttpContext context, string request)
    {
        var header = GetSerializedHeader(context);

        if(context.Request?.ContentType != null && !context.Request.ContentType.Contains("json"))
        {
            request = context.Request.Path != "/v1/Tokens" ? request : string.Empty;

            request = JsonSerializer.Serialize(new Dictionary<string, string> {
                { "message" , request }
            });
        }

        return new ApiRequest
        {
            Host = context.Request.Host.Value,
            IPAdress = context.Connection.RemoteIpAddress?.ToString(),
            Method = context.Request.Method,
            Path = context.Request.Path,
            Querystring = context.Request.QueryString.Value,
            RequestBody = request,
            Scheme = context.Request.Scheme,
            Header = header
        };
    }

    private static string GetSerializedHeader(HttpContext context)
    {
        var arrLenght = context.Request.Headers.Count;
        var headers = new KeyValuePair<string, StringValues>[arrLenght];
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
        if (context.Response?.ContentType is null || !context.Response.ContentType.Contains("json"))
        {
            response = JsonSerializer.Serialize(new Dictionary<string, string> {
                { "message" , response }
            });
        }
        response = context?.Request?.Path == "/v1/ThreeDS/init3ds" ? string.Empty : response;

        return new ApiResponse
        {
            ElapsedTime = elapsedMiliseconds,
            Response = response,
            StatusCode = context.Response.StatusCode,
        };
    }
}