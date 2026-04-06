using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;

public interface IAccountHttpClient
{
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task<string> GetPasswordResetTokenAsync(SendForgotPasswordEmailRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> CheckBirthdateAllowedRange(CheckBirthDateRequest request);
}