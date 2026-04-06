using LinkPara.Emoney.ApiGateway.Authentication;
using LinkPara.Emoney.ApiGateway.Models.Requests;
using LinkPara.Emoney.ApiGateway.Models.Responses;
using LinkPara.Emoney.ApiGateway.Services.HttpClients;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.ApiGateway.Controllers;

public class SignaturesController : ApiControllerBase
{
    private readonly IVaultClient _vaultClient;
    private readonly IApiKeyHttpClient _apiKeyHttpClient;
    private readonly IHashGenerator _hashGenerator;
    private readonly IEncryptionService _encryptionService;

    public SignaturesController(IEncryptionService encryptionService,
        IHashGenerator hashGenerator,
        IApiKeyHttpClient apiKeyHttpClient,
        IVaultClient vaultClient)
    {
        _encryptionService = encryptionService;
        _hashGenerator = hashGenerator;
        _apiKeyHttpClient = apiKeyHttpClient;
        _vaultClient = vaultClient;
    }

    /// <summary>
    /// Generate Hmac authentication signature
    /// and other required fields for authentication (test) (dev only)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("generate-test-signature")]
    public async Task<SignatureTestDataResponse> GenerateTestSignature(SignatureTestDataRequest request)
    {
        var apiKeys = await _apiKeyHttpClient.GetApiKeyAsync(request.PublicKey);

        if (apiKeys.Partner.PartnerNumber != request.PartnerNumber)
            throw new InvalidParameterException(request.PartnerNumber);

        var keyConstant = _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant", "EmoneyApiKeyEncryptionKey");
        var privateKey = _encryptionService.Decrypt(apiKeys.PrivateKey, keyConstant);
        var nonce = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

        var parameters = new PrivateKeyValidationParameters
        {
            Nonce = nonce.ToString(),
            PrivateKey = privateKey,
            PublicKey = request.PublicKey
        };

        var message = $"{parameters.PublicKey}{parameters.Nonce}";
        var securityData = _hashGenerator.Generate(message, parameters.PrivateKey);

        return new SignatureTestDataResponse
        {
            Nonce = nonce.ToString(),
            Signature = securityData,
            PartnerId = Guid.Parse(apiKeys.PartnerId),
            PublicKey = request.PublicKey
        };
    }
}
