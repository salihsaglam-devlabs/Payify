namespace LinkPara.ApiGateway.Services.Emoney;

public interface IPaymentOtpRequirementService
{
    Task<bool> IsRequireOtp(decimal amount);
}
