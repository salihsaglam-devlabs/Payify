using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;

public interface IAccountHttpClient
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task<string> GetPasswordResetTokenAsync(SendForgotPasswordEmailRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<GetEmailUpdateTokenResponse> GetEmailUpdateTokenAsync(GetEmailUpdateTokenRequest request);
    Task UpdateEmailAsync(UpdateEmailRequest request);
}