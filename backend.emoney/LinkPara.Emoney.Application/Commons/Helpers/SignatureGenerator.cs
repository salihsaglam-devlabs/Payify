using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using System.Text.Json;
using IHashGenerator = LinkPara.Security.IHashGenerator;

namespace LinkPara.Emoney.Application.Commons.Helpers;

public interface ISignatureGenerator
{
    Task<SignatureDataResponse> GenerateSignatureAsync(SignatureDataRequest request);
}

public class SignatureGenerator : ISignatureGenerator
{
    private readonly IHashGenerator _hashGenerator;
    private readonly ISecureRandomGenerator _randomGenerator;
    private readonly string _paymentProviderType;
    private readonly IVaultClient _vaultClient;
    private readonly IEncryptionService _encryptionService;
    private readonly IPaymentProviderServiceFactory _paymentProviderServiceFactory;
    private readonly Uri _pfServiceUrl;

    public SignatureGenerator(
        IHashGenerator hashGenerator,
        ISecureRandomGenerator randomGenerator,
        IVaultClient vaultClient,
        IPaymentProviderServiceFactory paymentProviderServiceFactory,
        IEncryptionService encryptionService)
    {
        _hashGenerator = hashGenerator;
        _randomGenerator = randomGenerator;
        _vaultClient = vaultClient;
        _paymentProviderServiceFactory = paymentProviderServiceFactory;
        _paymentProviderType = vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "Type");
        _encryptionService = encryptionService;
        var pfServiceUrl = vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Pf");
        _pfServiceUrl = new Uri(pfServiceUrl);
    }

    public async Task<SignatureDataResponse> GenerateSignatureAsync(SignatureDataRequest request)
    {
        var paymentProvider = await _paymentProviderServiceFactory.GetPaymentProviderServiceAsync(_paymentProviderType);

        var apiKeys = await paymentProvider.GetApiKeysAsync(request.PublicKey, _pfServiceUrl);

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

public class SignatureValidationParameters
{
    public string PrivateKey { get; set; }
    public string PublicKey { get; set; }
    public string Nonce { get; set; }
    public string ConversationId { get; set; }
    public string Signature { get; set; }
}
