using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi;

public static class VposHelper
{
    public static bool IsCardNumberValid(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
            return false;

        var cardNumberCheck = cardNumber.Length == 16
            ? new Regex(@"^\d{16}$")
            : new Regex(@"^\d{15}$");

        if (cardNumberCheck.IsMatch(cardNumber) == false)
            return false;

        var checksum = cardNumber.Select((c, i) => c - '0' << (cardNumber.Length - i - 1 & 1))
            .Sum(n => n > 9 ? n - 9 : n);

        return checksum % 10 == 0 && checksum > 0;
    }

    public static string GetSha1(string text)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding iso88599 = Encoding.GetEncoding("iso-8859-9");

        byte[] inputbytes;
        using (var sha = SHA1.Create())
        {
            inputbytes = sha.ComputeHash(iso88599.GetBytes(text));
        }
        return HttpUtility.UrlEncode(Convert.ToBase64String(inputbytes));
    }

    public static string GetSha1512(string text)
    {
        byte[] inputbytes;

        using (var sha = SHA512.Create())
        {
            inputbytes = sha.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(text));
        }
        return Convert.ToBase64String(inputbytes);
    }
    
    public static string GetSHA512String(string strData)
    {
        var message = Encoding.UTF8.GetBytes(strData);
        using (var alg = SHA512.Create())
        {
            var hashValue = alg.ComputeHash(message);
            string hex = BitConverter.ToString(hashValue).Replace("-", "").ToLower();
            var hexBytes = Encoding.UTF8.GetBytes(hex);
            return Convert.ToBase64String(hexBytes);
        }
    }

    public static string GetEncoding1254(string text)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        byte[] inputbytes;
        using (var sha = SHA512.Create())
        {
            Byte[] PasswordAsByte = Encoding.GetEncoding(1254).GetBytes(text);
            inputbytes = sha.ComputeHash(PasswordAsByte);
        }
        string base64Hash = Convert.ToBase64String(inputbytes);
        string urlEncodedHash = Uri.EscapeDataString(base64Hash);

        return urlEncodedHash;
    }

    public static string GetShaInit1512(string text)
    {
        byte[] inputbytes;

        using (var sha = SHA512.Create())
        {
            inputbytes = sha.ComputeHash(Encoding.GetEncoding("UTF-8").GetBytes(text));
        }
        return Convert.ToBase64String(inputbytes);
    }

    public static string GetSha256(string originalString)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(originalString));
            return Convert.ToBase64String(bytes);
        }
    }

    public static string GetHexSha256(string originalString)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(originalString));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public static string HashToString(string serializedModel, string secretKey)
    {
        using (var hmacsha512 = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
        {
            byte[] hashByteArray = hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(serializedModel));
            return Convert.ToBase64String(hashByteArray);
        }
    }

    public static T ParseHelper<T>(string jsonInput)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new LongOrStringConverter() }
            };

            return JsonSerializer.Deserialize<T>(jsonInput, options);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Failed to parse JSON: {ex.Message}");
            throw;
        }
    }
    public class LongOrStringConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.TokenType switch
            {
                JsonTokenType.String => long.TryParse(reader.GetString(), out var result)
                    ? result
                    : throw new JsonException($"Invalid format for long field: {reader.GetString()}"),
                JsonTokenType.Number => reader.GetInt64(),
                _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
            };
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
