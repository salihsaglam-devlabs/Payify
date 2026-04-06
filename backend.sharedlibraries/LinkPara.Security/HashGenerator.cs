using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace LinkPara.Security;

public class HashGenerator : IHashGenerator
{
    private readonly ILogger<HashGenerator> _logger;

    public HashGenerator(ILogger<HashGenerator> logger)
    {
        _logger = logger;
    }
    
    public string Generate(string message, string key)
    {
        try
        {
            using var hmac = new HMACSHA256(Convert.FromBase64String(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToBase64String(hash);
        }
        catch (Exception exception)
        {
            _logger.LogError($"HashGeneratorError : {exception}");
            throw;
        }
    }
}