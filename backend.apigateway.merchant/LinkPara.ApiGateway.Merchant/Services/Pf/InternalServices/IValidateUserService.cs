namespace LinkPara.ApiGateway.Merchant.Services.Pf.InternalServices;

public interface IValidateUserService
{
    Task ValidateUserAsync(string publicKey, string userId);
}
