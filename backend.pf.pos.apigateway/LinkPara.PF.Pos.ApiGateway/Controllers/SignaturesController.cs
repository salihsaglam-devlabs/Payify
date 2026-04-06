using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Pos.ApiGateway.Authentication;
using LinkPara.PF.Pos.ApiGateway.Models.Requests;
using LinkPara.PF.Pos.ApiGateway.Models.Responses;
using LinkPara.PF.Pos.ApiGateway.Services.HttpClients;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.Pos.ApiGateway.Controllers;

public class SignaturesController : ApiControllerBase
{
    private readonly IMerchantDeviceHttpClient _merchantDeviceHttpClient;
    private readonly IEncryptionService _encryptionService;
    private readonly IVaultClient _vaultClient;
    private readonly IHashGenerator _hashGenerator;

    public SignaturesController(IMerchantDeviceHttpClient merchantDeviceHttpClient,
        IHashGenerator hashGenerator,
        IEncryptionService encryptionService,
        IVaultClient vaultClient)
    {
        _merchantDeviceHttpClient = merchantDeviceHttpClient;
        _hashGenerator = hashGenerator;
        _encryptionService = encryptionService;
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
        string productionAccessKey = null;

        try
        {
            productionAccessKey = _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant",
                "PFProductionSignatureAccessKey");
        }
        catch (Exception ex)
        {
            productionAccessKey = null;
        }

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        if (environment?.ToLowerInvariant() == "production" && request.ProductionAccessKey != productionAccessKey)
        {
            throw new ForbiddenAccessException();
        }

        var apiKeys = await _merchantDeviceHttpClient.GetDeviceApiKeysAsync(request.PublicKey);

        if (apiKeys.SerialNumber != request.SerialNumber)
            throw new InvalidParameterException(request.SerialNumber);

        var keyConstant =
            _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant", "PfApiKeyEncryptionKey");
        var privateKey = _encryptionService.Decrypt(apiKeys.PrivateKeyEncrypted, keyConstant);
        var nonce = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

        var parameters = new SignatureValidationParameters
        {
            Nonce = nonce.ToString(),
            PrivateKey = privateKey,
            PublicKey = request.PublicKey,
            ConversationId = !string.IsNullOrEmpty(request.ConversationId)
                ? request.ConversationId
                : new Random().Next().ToString(),
        };

        var message = $"{parameters.PublicKey}{parameters.Nonce}";
        var securityData = _hashGenerator.Generate(message, parameters.PrivateKey);

        var secondMessage = $"{parameters.PrivateKey}{parameters.ConversationId}{parameters.Nonce}{securityData}";
        var signature = _hashGenerator.Generate(secondMessage, parameters.PrivateKey);

        return new SignatureTestDataResponse
        {
            Nonce = nonce.ToString(),
            Signature = signature,
            MerchantId = apiKeys.MerchantId,
            SerialNumber = apiKeys.SerialNumber,
            PublicKey = request.PublicKey,
            ConversationId = parameters.ConversationId
        };
    }
}