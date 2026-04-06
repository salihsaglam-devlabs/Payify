namespace LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Responses;

public class VerifyOtpResponse
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }

    public VerifyOtpResponse(bool success, string errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }
}