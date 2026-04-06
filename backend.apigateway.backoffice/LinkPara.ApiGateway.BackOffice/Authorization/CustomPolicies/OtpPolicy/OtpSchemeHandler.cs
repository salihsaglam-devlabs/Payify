using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace LinkPara.ApiGateway.BackOffice.Authorization.CustomPolicies.OtpPolicy
{
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
            if (_context.Response.StatusCode != (int)HttpStatusCode.OK)
            {
                _logger.LogError("SchemaHandler returned :" + _context.Response.StatusCode + " " + _context.Response.Body) ;
            }
            
            if (_context.Response.StatusCode == (int)HttpStatusCode.Unauthorized) return;

            Console.WriteLine("SchemaHandler returned : " + _context.Response.StatusCode);
            
            _context.Response.StatusCode = 401;

            var details = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "InvalidOTP",
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
}
