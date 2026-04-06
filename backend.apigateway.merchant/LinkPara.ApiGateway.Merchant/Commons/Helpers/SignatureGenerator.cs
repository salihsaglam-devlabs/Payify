using LinkPara.Security;
using LinkPara.HttpProviders.Vault;
using LinkPara.ApiGateway.Merchant.Authentication;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

namespace LinkPara.ApiGateway.Merchant.Commons.Helpers;

public interface ISignatureGenerator
{
    Task<SignatureDataResponse> GenerateSignatureAsync(SignatureDataRequest request);
}

public class SignatureGenerator : ISignatureGenerator
{
    private readonly IMerchantHttpClient _merchantHttpClient;
    private readonly IEncryptionService _encryptionService;
    private readonly IVaultClient _vaultClient;
    private readonly IHashGenerator _hashGenerator;
    private readonly ISecureRandomGenerator _randomGenerator;

    public SignatureGenerator(HttpClient client, IHttpContextAccessor httpContextAccessor,
        IMerchantHttpClient merchantHttpClient,
        IHashGenerator hashGenerator,
        IEncryptionService encryptionService,
        IVaultClient vaultClient,
        ISecureRandomGenerator randomGenerator)
    {
        _merchantHttpClient = merchantHttpClient;
        _hashGenerator = hashGenerator;
        _encryptionService = encryptionService;
        _vaultClient = vaultClient;
        _randomGenerator = randomGenerator;
    }
    public async Task<SignatureDataResponse> GenerateSignatureAsync(SignatureDataRequest request)
    {
        var apiKeys = await _merchantHttpClient.GetApiKeysAsync(request.PublicKey);
        var keyConstant = _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant", "PfApiKeyEncryptionKey");
        var privateKey = _encryptionService.Decrypt(apiKeys.PrivateKeyEncrypted, keyConstant);
        var nonce = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

        var parameters = new SignatureValidationParameters
        {
            Nonce = nonce.ToString(),
            PrivateKey = privateKey,
            PublicKey = request.PublicKey,
            ConversationId = !string.IsNullOrEmpty(request.ConversationId) 
                                    ? request.ConversationId 
                                    : _randomGenerator.GenerateSecureRandomNumber(1, 10000000000).ToString(),
        };

        var message = $"{parameters.PublicKey}{parameters.Nonce}";
        var securityData = _hashGenerator.Generate(message, parameters.PrivateKey);

        var secondMessage = $"{parameters.PrivateKey}{parameters.ConversationId}{parameters.Nonce}{securityData}";
        var signature = _hashGenerator.Generate(secondMessage, parameters.PrivateKey);

        return new SignatureDataResponse
        {
            Nonce = nonce.ToString(),
            Signature = signature,
            MerchantId = apiKeys.MerchantId,
            PublicKey = request.PublicKey,
            ConversationId = parameters.ConversationId
        };
    }
}
