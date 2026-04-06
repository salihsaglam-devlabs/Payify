using LinkPara.ApiGateway.Merchant.Commons.Models.IdentityModels;
using LinkPara.ApiGateway.Merchant.Services.Notification.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Notification.Models.Requests;
using LinkPara.HttpProviders.Identity.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;

namespace LinkPara.ApiGateway.Merchant.Authorization.CustomPolicies.OtpPolicy;

public class OtpRequirementHandler : AuthorizationHandler<OtpRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOtpHttpClient _otpHttpClient;
    private readonly IConfiguration _configuration;

    public OtpRequirementHandler(IHttpContextAccessor httpContextAccessor,
        IOtpHttpClient otpHttpClient,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _otpHttpClient = otpHttpClient;
        _configuration = configuration;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OtpRequirement requirement)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environment?.ToLowerInvariant() == "development")
        {
            context.Succeed(requirement);
        }

        var headers = _httpContextAccessor.HttpContext!.Request.Headers;

        if (!headers.ContainsKey("otp-code")
            || !headers.ContainsKey("otp-authorization-id")
            || !headers.ContainsKey("otp-timestamp"))
        {
            return;
        }

        if (!int.TryParse(headers["otp-code"], out var code))
        {
            return;
        }

        if (environment?.ToLowerInvariant() == "staging")
        {
            if (await CheckDefaultUser(context, requirement, code))
            {
                context.Succeed(requirement);
                return;
            }
        }

        try
        {
            var result = await _otpHttpClient.VerifyOtpAsync(new VerifyOtpRequest
            {
                Code = code,
                TimeStamp = headers["otp-timestamp"],
                OtpAuthorizationId = headers["otp-authorization-id"]
            });

            if (!result.Success)
            {
                _httpContextAccessor.HttpContext.Items["OTPError"] = result.ErrorMessage;
                context.Fail(new AuthorizationFailureReason(this, result.ErrorMessage));
                return;
            }

            context.Succeed(requirement);

        }
        catch (Exception e)
        {
            context.Fail(new AuthorizationFailureReason(this, e.Message));
        }
    }
    private async Task<bool> CheckDefaultUser(AuthorizationHandlerContext context, OtpRequirement requirement, int code)
    {
        var request = _httpContextAccessor.HttpContext!.Request;
        request.EnableBuffering();

        using (var streamReader = new StreamReader(request.Body, leaveOpen: true))
        {
            var body = await streamReader.ReadToEndAsync();
            request.Body.Position = 0;

            var loginRequest = JsonConvert.DeserializeObject<LoginRequest>(body);

            var defaultUser = new List<DefaultUser>();
            var defaultUsers = _configuration.GetSection("DefaultUsers").Get<List<DefaultUser>>();

            if (loginRequest != null)
            {
                var matchingUser = defaultUsers.FirstOrDefault(user => user.PhoneNumber == loginRequest.Username && user.Otp == code);

                if (matchingUser != null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}