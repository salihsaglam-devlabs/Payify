using LinkPara.Card.Application.Commons.Models.AppConfiguration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.API.Helpers.Security;

public class AuthBypass
{
    private readonly RequestDelegate _next;
    private readonly CardConfigOptions.AuthBypassSection _options;

    public AuthBypass(RequestDelegate next, IOptions<CardConfigOptions> options)
    {
        _next = next;
        _options = options.Value.Application.AuthBypass;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var bypassEnabled = _options.Enabled ?? false;

        if (bypassEnabled)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint is not null)
            {
                var cad = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                var controllerName = cad?.ControllerName;

                var bypassControllers = _options.Controllers ?? Array.Empty<string>();

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