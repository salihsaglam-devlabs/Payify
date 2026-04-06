using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.Security;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class ApiKeyGenerator : IApiKeyGenerator
{
    private readonly ILogger<ApiKeyGenerator> _logger;
    private readonly ISecureKeyGenerator _secureKeyGenerator;
    private readonly IEncryptionService _encryptionService;
    private readonly IVaultClient _vaultClient;
    private const int PublicKeyLength = 16;
    private const int PrivateKeyLength = 32;

    public ApiKeyGenerator(ILogger<ApiKeyGenerator> logger, ISecureKeyGenerator secureKeyGenerator,
        IEncryptionService encryptionService,
        IVaultClient vaultClient)
    {
        _logger = logger;
        _secureKeyGenerator = secureKeyGenerator;
        _encryptionService = encryptionService;
        _vaultClient = vaultClient;
    }

    public Task<MerchantApiKeyDto> Generate(Guid merchantId)
    {
        MerchantApiKeyDto merchantApiKeyDto = new();  
        try
        {
            var keyConstant = _vaultClient.GetSecretValue<string>("SharedSecrets", "SignatureKeyConstant", "PfApiKeyEncryptionKey");
            var publicKey = _secureKeyGenerator.Generate(PublicKeyLength);
            var privateKey = _secureKeyGenerator.Generate(PrivateKeyLength);
            var encrypt = _encryptionService.Encrypt(privateKey, keyConstant);

            merchantApiKeyDto.MerchantId = merchantId;
            merchantApiKeyDto.PrivateKeyEncrypted = encrypt == string.Empty ? null : encrypt;
            merchantApiKeyDto.PublicKey = publicKey == string.Empty ? null : publicKey;
            merchantApiKeyDto.PrivateKey = privateKey;
        }
        catch (Exception exception)
        {
            _logger.LogError($"GenerateMerchantApiKeyError : {exception}");
        }

        return Task.FromResult(merchantApiKeyDto);        
    }
}
