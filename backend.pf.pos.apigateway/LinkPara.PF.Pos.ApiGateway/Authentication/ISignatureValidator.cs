using LinkPara.PF.Pos.ApiGateway.Models.Responses;

namespace LinkPara.PF.Pos.ApiGateway.Authentication;

public interface ISignatureValidator
{
    Task<SignatureValidationResponse> ValidateAsync(SignatureValidationParameters validationParameters);
}