using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace LinkPara.Security.UnitTest;

using static Testing;

public class EncryptionTests
{
    private const string Key = "ApiKeyEncryptKey";

    [Test]
    public void ShouldEncryptDecrypt()
    {
        using var scope = ScopeFactory.CreateScope();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
        
        var text = "6JUoGeB7L3vAbNK3e9yM9Bt18MWB28GW0TpL9BzJxaE=";
        
        var cipherText = encryptionService.Encrypt(text, Key);
        var plainText =  encryptionService.Decrypt(cipherText, Key);
        text.Should().Be(plainText);
    }
    
    [Test]
    public void ShouldEncryptDecryptLongText()
    {
        using var scope = ScopeFactory.CreateScope();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
        
        var text = "Long Text";

        var builder = new StringBuilder(text);

        for (var counter = 0; counter < 100000; counter++)
        {
            builder.Append(text);
        }

        var longText = builder.ToString();
        var cipherText =  encryptionService.Encrypt(longText, Key);
        var plainText =  encryptionService.Decrypt(cipherText, Key);
        longText.Should().Be(plainText);
    }
}