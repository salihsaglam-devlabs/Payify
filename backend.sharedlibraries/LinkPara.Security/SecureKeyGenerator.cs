using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace LinkPara.Security;

public class SecureKeyGenerator : ISecureKeyGenerator
{
    private readonly ILogger<SecureKeyGenerator> _logger;

    public SecureKeyGenerator(ILogger<SecureKeyGenerator> logger)
    {
        _logger = logger;
    }
    
    public string Generate(int length)
    {
        if (length <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }
        
        try
        {
            var randomBytes = new byte[length];
        
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomBytes);
            var key = Convert.ToBase64String(randomBytes);
        
            return key;
        }
        catch (Exception exception)
        {
            _logger.LogError($"SecureKeyGeneratorError : {exception}");
            throw;
        }
    }
}