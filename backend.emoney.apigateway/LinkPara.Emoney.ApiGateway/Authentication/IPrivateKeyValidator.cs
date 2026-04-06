using LinkPara.Emoney.ApiGateway.Models.Responses;

namespace LinkPara.Emoney.ApiGateway.Authentication
{
    public interface IPrivateKeyValidator
    {
        Task<PrivateKeyValidationResponse> ValidateAsync(PrivateKeyValidationParameters validationParameters);
    }
}
