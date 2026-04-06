using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Identity.HttpClients;

public interface IAccountHttpClient
{
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task<string> GetPasswordResetTokenAsync(SendForgotPasswordEmailRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
}