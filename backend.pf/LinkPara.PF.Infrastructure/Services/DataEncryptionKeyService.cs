using LinkPara.Cache;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.Security;
using LinkPara.SharedModels.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Infrastructure.Services
{
    public class DataEncryptionKeyService : IDataEncryptionKeyService
    {
        private readonly ICacheService _cacheService;
        private readonly IVaultClient _vaultClient;
        private readonly IRsaEncryptionService _rsaEncryptionService;
        public DataEncryptionKeyService(ICacheService cacheService,
            IRsaEncryptionService rsaEncryptionService,
            IVaultClient vaultClient) {
            _rsaEncryptionService = rsaEncryptionService;
           _cacheService = cacheService;
           _vaultClient = vaultClient;
        }

        public async Task<string> GetDataEncryptionKeyAsync()
        {
            return  await _cacheService.GetOrCreateAsync("SecretKey", async () =>
            {
                var dekEncrypted = await _vaultClient.GetSecretValueAsync<string>("PFSecrets", "DataEncryptionKey", "Key");
                var privateKey = await _vaultClient.GetSecretValueAsync<string>("PFSecrets", "RSAConfig", "PrivateKey");
                return _rsaEncryptionService.Decrypt(dekEncrypted, privateKey);
            }); 
        }
    }
}
