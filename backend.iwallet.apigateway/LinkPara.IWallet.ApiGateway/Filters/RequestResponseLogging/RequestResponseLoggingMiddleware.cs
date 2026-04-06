using System.Diagnostics;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using MassTransit;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IO;

namespace LinkPara.IWallet.ApiGateway.Filters.RequestResponseLogging;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    private readonly RequestResponseLoggingSettings _requestResponseLoggingSettings;
    private readonly IBus _eventBus;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

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
            CancellationTokenSource token = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await _eventBus.Publish<RequestResponseLogCreated>(requestResponseLog, token.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"RequestResponseLoggingMiddleware detail: \n{exception}");
            throw;
        }
    }

    private ApiRequest InitRequestLog(HttpContext context, string request)
    {
        var paramQueryString = !requestIsSensitive ? context.Request.QueryString.Value : "sensitive!";
        var paramBody = !requestIsSensitive ? request : "sensitive!";

        return new ApiRequest
        {
            Host = context.Request.Host.Value,
            IPAdress = context.Connection.RemoteIpAddress.ToString(),
            Method = context.Request.Method,
            Path = context.Request.Path,
            Querystring = paramQueryString,
            RequestBody = paramBody,
            Scheme = context.Request.Scheme,
        };
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