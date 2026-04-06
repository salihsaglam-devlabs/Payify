using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.Cache;
using LinkPara.HttpProviders.Vault;
using LinkPara.Security;
using MediatR;
using System.Text.Json;

namespace LinkPara.PF.Application.Features.DataEncryptionKey.Commands.DataEncryptionKey
{
    public class DataEncryptionKeyCommand : IRequest
    {

    }
    public class DataEncryptionKeyCommandHandler : IRequestHandler<DataEncryptionKeyCommand>
    {
        private const int DataEncryptionKeyLength = 24;
        private readonly ICacheService _cacheService;
        private readonly ISecureKeyGenerator _keyGenerator;
        private readonly IRsaEncryptionService _rsaEncryptionService;
        private readonly IAuditLogService _auditLogService;
        private readonly IVaultClient _vaultClient;

        public DataEncryptionKeyCommandHandler(
            ICacheService cacheService,
            ISecureKeyGenerator keyGenerator,
            IVaultClient vaultClient,
            IRsaEncryptionService rsaEncryptionService,
            IAuditLogService auditLogService)
        {
            _cacheService = cacheService;
            _keyGenerator = keyGenerator;
            _vaultClient = vaultClient;
            _rsaEncryptionService = rsaEncryptionService;
            _auditLogService = auditLogService;
        }

        public async Task<Unit> Handle(DataEncryptionKeyCommand request, CancellationToken cancellationToken)
        {

            var dataEncryptionKey = await _vaultClient.GetSecretValueAsync<string>("PFSecrets", "DataEncryptionKey", "Key");

            if (dataEncryptionKey is not null)
            {
                SetCardSecretKeyCache(dataEncryptionKey);
            }
            else
            {
                await InitialCardSecretKey();
            }

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "CardEncryptedSecretKey",
                    SourceApplication = "PF",
                    Resource = "CardSecretKey",
                    Details = new Dictionary<string, string>
                    {
                    {"SecretKey", "Success EncryptedSecretKey , Success Set Cache"}
                    }
                });

            return Unit.Value;
        }
        private async Task InitialCardSecretKey()
        {
            var cardSecretKey = _keyGenerator.Generate(DataEncryptionKeyLength);

            var publicKey = await RsaGenerateKeyAsync();

            var dek = await GenerateDataEncryptionKeyAsync(cardSecretKey, publicKey);

            if (dek is not null)
            {
                _cacheService.Add("SecretKey", cardSecretKey);
            }

        }
        private async Task<string> GenerateDataEncryptionKeyAsync(string cardSecretKey, string publicKey)
        {
            var encryptedSecretKey = _rsaEncryptionService.Encrypt(cardSecretKey, publicKey);

            var kvDataEncryptionKey = new
            {
                data = new
                {
                    Key = encryptedSecretKey,
                }
            };
            var jsonPayloadDEK = JsonSerializer.Serialize(kvDataEncryptionKey);

            await _vaultClient.PostSecretValueAsync<string>("PFSecrets", "DataEncryptionKey", jsonPayloadDEK);
            return encryptedSecretKey;
        }
        private async Task<string> RsaGenerateKeyAsync()
        {
            var rsaKeys = _rsaEncryptionService.GenerateKey();

            var kvDataRsaConfig = new
            {
                data = new
                {
                    PrivateKey = rsaKeys.PrivateKey,
                    PublicKey = rsaKeys.PublicKey
                }
            };

            var jsonPayloadKvData = JsonSerializer.Serialize(kvDataRsaConfig);

             await _vaultClient.PostSecretValueAsync<string>("PFSecrets", "RSAConfig", jsonPayloadKvData);
            return rsaKeys.PublicKey;
        }
        private void SetCardSecretKeyCache(string encryptedSecretKey)
        {
            var privateKey = _vaultClient.GetSecretValue<string>("PFSecrets", "RSAConfig", "PrivateKey");
            var kek = _rsaEncryptionService.Decrypt(encryptedSecretKey, privateKey);

            if (kek is not null)
            {
                _cacheService.Add("SecretKey", kek);
            }
        }
    }

}