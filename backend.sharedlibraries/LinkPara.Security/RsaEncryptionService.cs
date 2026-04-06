using LinkPara.Security.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Security
{
    public class RsaEncryptionService : IRsaEncryptionService
    {
        private readonly ILogger<RsaEncryptionService> _logger;

        public RsaEncryptionService(ILogger<RsaEncryptionService> logger)
        {
            _logger = logger;
        }

        public string Decrypt(string encryptedText, string privateKey)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
                throw new ArgumentNullException(nameof(encryptedText));
            if (string.IsNullOrWhiteSpace(privateKey))
                throw new ArgumentNullException(nameof(privateKey));
            try
            {
                using RSA rsa = RSA.Create();
                rsa.FromXmlString(privateKey);

                byte[] encryptedData = Convert.FromBase64String(encryptedText);
                byte[] decryptedData = rsa.Decrypt(encryptedData, RSAEncryptionPadding.Pkcs1);

                return Encoding.UTF8.GetString(decryptedData);
            }
            catch (Exception exception)
            {
                _logger.LogError($"RsaDecryptError : {exception}");
                throw;
            }
        }

        public string Encrypt(string plainText, string publicKey)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                throw new ArgumentNullException(nameof(plainText));
            if (string.IsNullOrWhiteSpace(publicKey))
                throw new ArgumentNullException(nameof(publicKey));
            try
            {
                using RSA rsa = RSA.Create();
                rsa.FromXmlString(publicKey);

                byte[] data = Encoding.UTF8.GetBytes(plainText);
                byte[] encryptedData = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
                return Convert.ToBase64String(encryptedData);
            }
            catch (Exception exception)
            {
                _logger.LogError($"RsaDecryptError : {exception}");
                throw;
            }


        }

        public RsaConfig GenerateKey()
        {
            using (RSA rsa = RSA.Create())
            {
                string privateKey = rsa.ToXmlString(true);

                string publicKey = rsa.ToXmlString(false);

                return new RsaConfig()
                {
                    PrivateKey = privateKey,
                    PublicKey = publicKey
                };
            }
        }
    }

}
