using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public interface IAccountHttpClient
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterWithUserName request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task<string> GetPasswordResetTokenAsync(SendForgotPasswordEmailRequest request);
    Task<GetResetPasswordTokenResponse> GetPasswordResetTokenAndEmailAsync(GetPasswordResetTokenAndEmailRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<GetEmailUpdateTokenResponse> GetEmailUpdateTokenAsync(GetEmailUpdateTokenRequest request);
    Task UpdateEmailAsync(UpdateEmailRequest request);
    Task<bool> CheckBirthdateAllowedRange(CheckBirthDateRequest request);
    Task<GetPhoneNumberTokenResponse> GetPhoneNumberUpdateTokenAsync(GetPhoneNumberUpdateTokenRequest request);
    Task UpdatePhoneNumberAsync(UpdatePhoneNumberRequest request);
}