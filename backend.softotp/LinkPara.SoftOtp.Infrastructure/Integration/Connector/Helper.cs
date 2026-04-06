using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

#pragma warning disable SYSLIB0023

// ReSharper disable SuggestBaseTypeForParameter
#pragma warning disable SYSLIB0021
#pragma warning disable SYSLIB0022
#pragma warning disable CS0618

namespace LinkPara.SoftOtp.Infrastructure.Integration.Connector
{
    /// <summary>
    /// Common utils. 
    /// </summary>
    public static class Helper
    {
        
        public static PowerFactorRequest EncryptRequest(string request, PowerfactorConfig config)
        {
            var pwfRequest = new PowerFactorRequest();

            var aesIvString = GenerateRandomKey();
            var aesKeyString = GenerateRandomKey();
            var token = aesIvString + aesKeyString;

            pwfRequest.CipherText = AesEncryption(request, aesIvString, aesKeyString);

            pwfRequest.SecretKey = RsaEncryption(token, config);

            pwfRequest.VerificationSignature = Sign(pwfRequest.CipherText, config);

            return pwfRequest;
        }


        private const string RandomKeyChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        private static string GenerateRandomKey(int count = 16)
        {
            using var rng = new RNGCryptoServiceProvider();
            
            var data = new byte[count];
            rng.GetBytes(data);
            
            var allowable = RandomKeyChars.ToCharArray();
            var l = allowable.Length;
            var chars = new char[count];

            for (var i = 0; i < count; i++)
            {
                chars[i] = allowable[data[i] % l];
            }

            return new string(chars);
        }
        
        private static string RsaEncryption(string data, PowerfactorConfig config)
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(config.PublicKey);
            
            var byt = Encoding.UTF8.GetBytes(data);
            var strModified = Convert.ToBase64String(byt);
            var bytesPlainTextData = Convert.FromBase64String(strModified);
            var bytesCypherText = rsa.Encrypt(bytesPlainTextData, RSAEncryptionPadding.OaepSHA1);
            
            return Convert.ToBase64String(bytesCypherText);
        }
        
        public static string AesEncryption(string plainText, string aesIvString, string aesKeyString)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var keyBytes = Encoding.ASCII.GetBytes(aesKeyString);
            var ivBytes = Encoding.ASCII.GetBytes(aesIvString);

            var cipher = new GcmBlockCipher(new AesEngine());

            var parameters = new AeadParameters(new KeyParameter(keyBytes), 128, ivBytes);
            cipher.Init(true, parameters);

            var cipherText = new byte[cipher.GetOutputSize(plainTextBytes.Length)];
            var len = cipher.ProcessBytes(plainTextBytes, 0, plainTextBytes.Length, cipherText, 0);
            len += cipher.DoFinal(cipherText, len);

            return Convert.ToBase64String(cipherText, 0, len);
        }
        
        private static string Sign(string cipherText, PowerfactorConfig config, string hashAlgorithm = "SHA256")
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(config.PrivateKey);
            var hash = rsa.SignData(Encoding.UTF8.GetBytes(cipherText), (HashAlgorithm)CryptoConfig.CreateFromName(hashAlgorithm));
            return Convert.ToBase64String(hash);
        }
        
        public static bool VerifySign(string cipherText, string verificationSignature, PowerfactorConfig config,
            string hashAlgorithm = "SHA256")
        {
            using var rsaProvider = new RSACryptoServiceProvider();
            
            rsaProvider.ImportParameters(GetRsaParametersByXmlString(config.PublicKey));
            var cipherTextBytes = Encoding.UTF8.GetBytes(cipherText);
            var cipherTextHashBytes = Convert.FromBase64String(verificationSignature);
            var isVerified = rsaProvider.VerifyData(cipherTextBytes, (HashAlgorithm)CryptoConfig.CreateFromName(hashAlgorithm), cipherTextHashBytes);
            return isVerified;
        }
        
        public static string RsaDecryption(string data, PowerfactorConfig config)
        {
            var rsa = new RSACryptoServiceProvider();
            
            rsa.FromXmlString(config.PrivateKey);
            var bytesPlainTextData = Convert.FromBase64String(data);
            var result = rsa.Decrypt(bytesPlainTextData, RSAEncryptionPadding.OaepSHA1);
            return Encoding.UTF8.GetString(result);
        }
        
        public static string AesDecryption(String cipherText, String token, String salt = "")
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            
            var keyBytes = Encoding.ASCII.GetBytes(token.Substring(16, 16));
            var ivBytes = Encoding.ASCII.GetBytes(token.Substring(0, 16));
            var saltBytes = Encoding.ASCII.GetBytes(salt);
            
            var cipher = new GcmBlockCipher(new AesEngine());

            var parameters = new AeadParameters(new KeyParameter(keyBytes), 128, ivBytes, saltBytes);
            cipher.Init(false, parameters);

            var plainText = new byte[cipher.GetOutputSize(cipherTextBytes.Length)];
            var len = cipher.ProcessBytes(cipherTextBytes, 0, cipherTextBytes.Length, plainText, 0);
            len += cipher.DoFinal(plainText, len);
            return Encoding.UTF8.GetString(plainText, 0, len);
        }
        
        public static string ObjectToJson(object obj)
        {
            using var stream = new MemoryStream();
            var serializer = new DataContractJsonSerializer(obj.GetType());
            serializer.WriteObject(stream, obj);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
        
        public static object JsonToObject(Type type, string json)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var serializer = new DataContractJsonSerializer(type);
            return serializer.ReadObject(stream);
        }
        
        public static RSAParameters GetRsaParametersByXmlString(string xmlString)
        {
            var parameters = new RSAParameters();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            if (xmlDoc.DocumentElement != null && xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus":
                            parameters.Modulus = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "Exponent":
                            parameters.Exponent = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "P":
                            parameters.P = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "Q":
                            parameters.Q = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "DP":
                            parameters.DP = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "DQ":
                            parameters.DQ = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "InverseQ":
                            parameters.InverseQ = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                        case "D":
                            parameters.D = string.IsNullOrEmpty(node.InnerText)
                                ? null
                                : Convert.FromBase64String(node.InnerText);
                            break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            return parameters;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

    }
}