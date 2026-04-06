namespace LinkPara.ApiGateway.BackOffice.Filters.MakerChecker;

public static class MakerCheckerMiddlewareExtension
{
    public static IApplicationBuilder UseMakerCheckerMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<MakerCheckerMiddleware>();
        return builder;
    }
}
