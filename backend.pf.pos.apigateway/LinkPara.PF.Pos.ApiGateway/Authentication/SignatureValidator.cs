using LinkPara.PF.Pos.ApiGateway.Models.Responses;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Pos.ApiGateway.Authentication;

public class SignatureValidator : ISignatureValidator
{
    private readonly IHashGenerator _hashGenerator;
    private const int TimestampExpirationInMinutes = 15;
    
    public SignatureValidator(IHashGenerator hashGenerator)
    {
        _hashGenerator = hashGenerator;
    }
    
    public async Task<SignatureValidationResponse> ValidateAsync(SignatureValidationParameters validationParameters)
    {
        if (string.IsNullOrWhiteSpace(validationParameters.Signature)
            || string.IsNullOrWhiteSpace(validationParameters.Nonce)
            || string.IsNullOrWhiteSpace(validationParameters.ConversationId)
            || string.IsNullOrWhiteSpace(validationParameters.PrivateKey)
            || string.IsNullOrWhiteSpace(validationParameters.PublicKey))
        {
            return new SignatureValidationResponse
            {
                IsSucceed = false,
                ErrorMessage = "InvalidHeader"
            };
        }

        if (!long.TryParse(validationParameters.Nonce, out var nonce))
        {
            return new SignatureValidationResponse
            {
                IsSucceed = false,
                ErrorMessage = "InvalidHeader"
            };
        }

        var timeStampExpired = await TimestampExpiredAsync(nonce);

        if (timeStampExpired)
        {
            return new SignatureValidationResponse
            {
                IsSucceed = false,
                ErrorMessage = "HmacTimestampExpired"
            };
        }
        
        var signatureMismatch = await CompareSignaturesAsync(validationParameters);

        if (signatureMismatch)
        {
            return new SignatureValidationResponse
            {
                IsSucceed = false,
                ErrorMessage = "SignatureMismatch"
            };
        }
        
        return new SignatureValidationResponse { IsSucceed = true };
    }

    private static Task<bool> TimestampExpiredAsync(long nonce)
    {
        var nonceInUtc = DateTimeOffset.FromUnixTimeMilliseconds(nonce);
        var currentUtc = DateTimeOffset.UtcNow;

        var difference = currentUtc - nonceInUtc;

        return Task.FromResult(difference >= TimeSpan.FromMinutes(TimestampExpirationInMinutes));
    }

    private Task<bool> CompareSignaturesAsync(SignatureValidationParameters parameters)
    {
        var message = $"{parameters.PublicKey}{parameters.Nonce}";
        var securityData = _hashGenerator.Generate(message, parameters.PrivateKey);

        var secondMessage = $"{parameters.PrivateKey}{parameters.ConversationId}{parameters.Nonce}{securityData}";
        var signatureCalculated = _hashGenerator.Generate(secondMessage, parameters.PrivateKey);

        return Task.FromResult(parameters.Signature != signatureCalculated);
    }
}