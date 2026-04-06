using LinkPara.ApiGateway.Boa.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Boa.Services.Identity.Models.Responses;
using LinkPara.Cache;
using LinkPara.SystemUser;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace LinkPara.ApiGateway.Boa.Filters.CustomerContext;

public class CustomerContextMiddleware
{
    private const int cacheMin = 5;
    private readonly RequestDelegate _next;
    private readonly IAuthHttpClient _authHttpClient;
    private readonly IApplicationUserService _userService;
    private readonly ICacheService _cacheService;

    public CustomerContextMiddleware(RequestDelegate next,
        IApplicationUserService userService,
        IAuthHttpClient authHttpClient,
        ICacheService cacheService)
    {
        _next = next;
        _userService = userService;
        _authHttpClient = authHttpClient;
        _cacheService = cacheService;
    }

    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        var requiresCustomer =
            endpoint?.Metadata.GetMetadata<CustomerContextRequiredAttribute>() != null;

        var tokenResponse = new GenerateTokenResponse();

        if (requiresCustomer)
        {
            var customerId = context.Request.Headers["X-CustomerId"].FirstOrDefault();
            var personId = context.Request.Headers["X-PersonId"].FirstOrDefault();

            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(personId))
            {
                await HandleErrorAsync(context, "CustomerId or PersonId required.");
                return;
            }

            try
            {
                var key = $"token-key-{customerId}-{personId}";

                tokenResponse = await _cacheService.GetOrCreateAsync(key, () =>
                    _authHttpClient.GenerateTokenAsync(new GenerateTokenRequest
                    {
                        ExternalCustomerId = customerId,
                        ExternalPersonId = personId
                    }), cacheMin);

            }
            catch (Exception exception)
            {
                await HandleErrorAsync(context, exception.Message);

                return;
            }
        }
        else
        {
            tokenResponse = new GenerateTokenResponse
            {
                AccessToken = _userService.Token,
                UserId = _userService.ApplicationUserId
            };
        }

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(tokenResponse.AccessToken);

        var identity = new ClaimsIdentity(jwtToken.Claims, "Custom");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, tokenResponse.UserId.ToString()));

        context.User = new ClaimsPrincipal(identity);

        context.Request.Headers["Authorization"] = $"Bearer {tokenResponse.AccessToken}";

        await _next(context);
    }

    private async Task HandleErrorAsync(HttpContext context, string errorMessage)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var details = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = errorMessage
        };

        var result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };

        await context.Response.WriteAsJsonAsync(details);
    }
}