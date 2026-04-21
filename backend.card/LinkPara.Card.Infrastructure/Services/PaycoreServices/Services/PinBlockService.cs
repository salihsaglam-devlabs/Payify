using LinkPara.Card.Application.Commons.Helpers;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.SecurityModels;
using LinkPara.Card.Infrastructure.Services.PaycoreServices.Models;
using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardPinServices.Services;

public class PinBlockService : IPinBlockService
{
    private readonly IConfiguration _configuration;
    private readonly IVaultClient _vaultClient;
    private readonly PaycoreSettings _paycoreSettings;

    public PinBlockService(IVaultClient vaultClient, IConfiguration configuration)
    {
        _vaultClient = vaultClient;
        _configuration = configuration;
        _paycoreSettings = new PaycoreSettings();
        _configuration.GetSection(nameof(PaycoreSettings)).Bind(_paycoreSettings);
        _paycoreSettings.VaultSettings =
            _vaultClient.GetSecretValue<PaycoreVaultSettings>("CardSecrets", "PaycoreSettings");
    }
    public async Task<EncDecPinblockResponse> GenerateEncryptedPinBlock(string clearPin, string clearCardNumber)
    {
        Validate(clearPin, clearCardNumber);

        string pinField = $"0{clearPin.Length}{clearPin}".PadRight(16, 'F');
        string pan12 = clearCardNumber.Substring(clearCardNumber.Length - 13, 12);
        string panField = $"0000{pan12}";

        byte[] pinBytes = HexHelper.HexToBytes(pinField);
        byte[] panBytes = HexHelper.HexToBytes(panField);

        byte[] clearPinBlock = new byte[8];
        for (int i = 0; i < 8; i++)
        {
            clearPinBlock[i] = (byte)(pinBytes[i] ^ panBytes[i]);
        }

        byte[] zpkBytes = HexHelper.HexToBytes(_paycoreSettings.VaultSettings.ClearZpkHex);
        byte[] normalizedKey = NormalizeKey(zpkBytes);

        using TripleDES tripleDes = TripleDES.Create();
        tripleDes.Mode = CipherMode.ECB;
        tripleDes.Padding = PaddingMode.None;
        tripleDes.Key = normalizedKey;

        using ICryptoTransform encryptor = tripleDes.CreateEncryptor();
        byte[] encryptedPinBlock = encryptor.TransformFinalBlock(clearPinBlock, 0, clearPinBlock.Length);

        return new EncDecPinblockResponse
        {
            IsSuccess = true,
            Data = HexHelper.BytesToHex(encryptedPinBlock)
        };
    }
    public async Task<EncDecPinblockResponse> DecryptEncryptedPinBlock(string encryptedBlock, string clearCardNumber)
    {
        try
        {
            ValidateEncryptedBlock(encryptedBlock, clearCardNumber);

            byte[] encryptedBytes = HexHelper.HexToBytes(encryptedBlock);

            byte[] zpkBytes = HexHelper.HexToBytes(_paycoreSettings.VaultSettings.ClearZpkHex);
            byte[] normalizedKey = NormalizeKey(zpkBytes);

            using TripleDES tripleDes = TripleDES.Create();
            tripleDes.Mode = CipherMode.ECB;
            tripleDes.Padding = PaddingMode.None;
            tripleDes.Key = normalizedKey;

            using ICryptoTransform decryptor = tripleDes.CreateDecryptor();
            byte[] decryptedBlock = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            string pan12 = clearCardNumber.Substring(clearCardNumber.Length - 13, 12);
            string panField = $"0000{pan12}";
            byte[] panBytes = HexHelper.HexToBytes(panField);

            byte[] clearDataBlock = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                clearDataBlock[i] = (byte)(decryptedBlock[i] ^ panBytes[i]);
            }

            string clearDataHex = HexHelper.BytesToHex(clearDataBlock);

            var data = ParseClearBlock(clearDataHex);

            return new EncDecPinblockResponse
            {
                IsSuccess = true,
                Data = data
            };
        }
        catch(Exception ex) {
            return new EncDecPinblockResponse
            {
                IsSuccess = false,
                Data = null,
                Description = ex.Message               
            };
        }
    }
    private static string ParseClearBlock(string clearDataHex)
    {
        if (string.IsNullOrWhiteSpace(clearDataHex) || clearDataHex.Length != 16)
            throw new ArgumentException("Çözülen veri bloğu geçersiz.");

        string lengthHex = clearDataHex.Substring(1, 1);

        if (!int.TryParse(lengthHex, System.Globalization.NumberStyles.HexNumber, null, out int dataLength))
            throw new ArgumentException("Veri uzunluğu çözümlenemedi.");

        return clearDataHex.Substring(2, dataLength);

    }
    private static void Validate(string clearPin, string clearCardNumber)
    {
        if (string.IsNullOrWhiteSpace(clearPin))
            throw new ArgumentException("ClearPin boş olamaz.");

        if (!clearPin.All(char.IsDigit))
            throw new ArgumentException("ClearPin sadece rakamlardan oluşmalıdır.");

        if (clearPin.Length < 4 || clearPin.Length > 12)
            throw new ArgumentException("ClearPin uzunluğu 4 ile 12 arasında olmalıdır.");

        if (string.IsNullOrWhiteSpace(clearCardNumber))
            throw new ArgumentException("Clear kart numarası boş olamaz.");

        if (!clearCardNumber.All(char.IsDigit))
            throw new ArgumentException("Clear kart numarası sadece rakamlardan oluşmalıdır.");

        if (clearCardNumber.Length < 13)
            throw new ArgumentException("Clear kart numarası geçersiz.");
    }
    private static void ValidateEncryptedBlock(string encryptedBlock, string clearCardNumber)
    {
        if (string.IsNullOrWhiteSpace(encryptedBlock))
            throw new ArgumentException("EncryptedBlock boş olamaz.");

        if (encryptedBlock.Length != 16)
            throw new ArgumentException("EncryptedBlock 16 hex karakter olmalıdır.");

        if (!encryptedBlock.All(Uri.IsHexDigit))
            throw new ArgumentException("EncryptedBlock yalnızca hex karakterlerden oluşmalıdır.");

        if (string.IsNullOrWhiteSpace(clearCardNumber))
            throw new ArgumentException("Clear kart numarası boş olamaz.");

        if (!clearCardNumber.All(char.IsDigit))
            throw new ArgumentException("Clear kart numarası sadece rakamlardan oluşmalıdır.");

        if (clearCardNumber.Length < 13)
            throw new ArgumentException("Clear kart numarası geçersiz.");
    }
    private static byte[] NormalizeKey(byte[] key)
    {
        if (key.Length == 16)
        {
            byte[] expanded = new byte[24];
            Array.Copy(key, 0, expanded, 0, 16);
            Array.Copy(key, 0, expanded, 16, 8);
            return expanded;
        }

        if (key.Length == 24)
            return key;

        throw new ArgumentException("ZPK 16 veya 24 byte olmalıdır.");
    }
}