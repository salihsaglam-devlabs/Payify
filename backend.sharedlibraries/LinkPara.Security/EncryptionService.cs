using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LinkPara.Security;

public class EncryptionService : IEncryptionService
{
    private readonly ILogger<EncryptionService> _logger;

    public EncryptionService(ILogger<EncryptionService> logger)
    {
        _logger = logger;
    }

    public string Encrypt(string plainText, string key)
    {
        // Check arguments.
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentNullException(nameof(plainText));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        try
        {
            var keyByte = Encoding.UTF8.GetBytes(key);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(keyByte, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"EncryptError : {exception}");
            throw;
        }
    }

    public string Decrypt(string cipherText, string key)
    {
        // Check arguments.
        if (string.IsNullOrWhiteSpace(cipherText))
            throw new ArgumentNullException(nameof(cipherText));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, fullCipher.Length - iv.Length);
            var keyBytes = Encoding.UTF8.GetBytes(key);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(keyBytes, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"DecryptError : {exception}");
            throw;
        }
    }
}