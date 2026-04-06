namespace LinkPara.ApiGateway.Boa.Filters.RequestResponseLogging;

public static class RequestResponseLoggingMiddlewareExtension
{
    public static IApplicationBuilder UseRequestResponseLoggingMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        return builder;
    }
}

