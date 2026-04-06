using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LinkPara.PF.Pos.ApiGateway.Authentication.SignaturePolicy;

public class SignatureSchemeHandler : IAuthenticationHandler
{
    private HttpContext _context;

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        _context = context;
        return Task.CompletedTask;
    }

    public Task<AuthenticateResult> AuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());

    public async Task ChallengeAsync(AuthenticationProperties properties)
    {
        _context.Response.StatusCode = 401;
        
        var details = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Hmac Authentication Error",
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1"
        };

        var result = new ObjectResult(details)
        {
            StatusCode = StatusCodes.Status401Unauthorized
        };
        
        await _context.Response.WriteAsync(JsonConvert.SerializeObject(result));
    }

    public Task ForbidAsync(AuthenticationProperties properties)
    {
        _context.Response.StatusCode = 403;
        return Task.CompletedTask;    
    }
}