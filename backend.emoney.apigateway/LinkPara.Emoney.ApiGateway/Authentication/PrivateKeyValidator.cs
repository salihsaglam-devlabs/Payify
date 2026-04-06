using LinkPara.Emoney.ApiGateway.Models.Responses;
using LinkPara.Security;

namespace LinkPara.Emoney.ApiGateway.Authentication;

public class PrivateKeyValidator : IPrivateKeyValidator
{
    private const int TimestampExpirationInMinutes = 15;
    private readonly IHashGenerator _hashGenerator;

    public PrivateKeyValidator(IHashGenerator hashGenerator)
    {
        _hashGenerator = hashGenerator;
    }

    public async Task<PrivateKeyValidationResponse> ValidateAsync(PrivateKeyValidationParameters validationParameters)
    {
        if (string.IsNullOrWhiteSpace(validationParameters.Signature)
           || string.IsNullOrWhiteSpace(validationParameters.Nonce)
           || string.IsNullOrWhiteSpace(validationParameters.PrivateKey)
           || string.IsNullOrWhiteSpace(validationParameters.PublicKey))
        {
            return new PrivateKeyValidationResponse
            {
                IsSucceed = false,
                ErrorMessage = "InvalidHeader"
            };
        }

        if (!long.TryParse(validationParameters.Nonce, out var nonce))
        {
            return new PrivateKeyValidationResponse
            {
                IsSucceed = false,
                ErrorMessage = "InvalidHeader"
            };
        }

        var timeStampExpired = await TimestampExpiredAsync(nonce);

        if (timeStampExpired)
        {
            return new PrivateKeyValidationResponse
            {
                IsSucceed = false,
                ErrorMessage = "HmacTimestampExpired"
            };
        }

        var signatureMismatch = await CompareSignaturesAsync(validationParameters);

        if (signatureMismatch)
        {
            return new PrivateKeyValidationResponse
            {
                IsSucceed = false,
                ErrorMessage = "SignatureMismatch"
            };
        }

        return new PrivateKeyValidationResponse { IsSucceed = true };

    }

    private static Task<bool> TimestampExpiredAsync(long nonce)
    {
        var nonceInUtc = DateTimeOffset.FromUnixTimeMilliseconds(nonce);
        var currentUtc = DateTimeOffset.UtcNow;

        var difference = currentUtc - nonceInUtc;

        return Task.FromResult(difference >= TimeSpan.FromMinutes(TimestampExpirationInMinutes));
    }

    private Task<bool> CompareSignaturesAsync(PrivateKeyValidationParameters parameters)
    {
        var message = $"{parameters.PublicKey}{parameters.Nonce}";
        var securityData = _hashGenerator.Generate(message, parameters.PrivateKey);

        return Task.FromResult(parameters.Signature != securityData);
    }
}
