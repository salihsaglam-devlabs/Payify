using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace LinkPara.ApiGateway.Merchant.Authorization.CustomPolicies.OtpPolicy;

public class OtpSchemeHandler : IAuthenticationHandler
{
    private HttpContext _context;
    private readonly ILogger<OtpSchemeHandler> _logger;

    public OtpSchemeHandler(ILogger<OtpSchemeHandler> logger)
    {
        _logger = logger;
    }

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        _context = context;
        return Task.CompletedTask;
    }

    public Task<AuthenticateResult> AuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());

    public async Task ChallengeAsync(AuthenticationProperties properties)
    {
        if (_context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            return;

        _context.Response.StatusCode = 401;

        var otpError = _context.Items["OTPError"] as string ?? "InvalidOTP";


        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = otpError,
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

        var result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };

        try
        {
            await _context.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
        catch (Exception exception)
        {
            _logger.LogError($"RequestResponseLoggingMiddleware detail: \n{exception}");
            throw;
        }
    }

    public Task ForbidAsync(AuthenticationProperties properties)
    {
        _context.Response.StatusCode = 403;
        return Task.CompletedTask;
    }
}