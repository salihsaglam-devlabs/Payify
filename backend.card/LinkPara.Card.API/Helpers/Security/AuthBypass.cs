using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace LinkPara.Card.API.Helpers.Security;

public class AuthBypass
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public AuthBypass(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var bypassEnabled = _configuration.GetValue<bool>("AuthBypass:Enabled");

        if (bypassEnabled)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint is not null)
            {
                var cad = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                var controllerName = cad?.ControllerName;

                var bypassControllers = _configuration
                    .GetSection("AuthBypass:Controllers")
                    .Get<string[]>() ?? Array.Empty<string>();

                var shouldBypass = !string.IsNullOrWhiteSpace(controllerName) &&
                                   bypassControllers.Contains(controllerName, StringComparer.OrdinalIgnoreCase);

                if (shouldBypass)
                {
                    var newMetadata = endpoint.Metadata.ToList();
                    newMetadata.Add(new AllowAnonymousAttribute());

                    var newEndpoint = new Endpoint(
                        endpoint.RequestDelegate!,
                        new EndpointMetadataCollection(newMetadata),
                        endpoint.DisplayName);

                    context.SetEndpoint(newEndpoint);
                }
            }
        }

        await _next(context);
    }
}