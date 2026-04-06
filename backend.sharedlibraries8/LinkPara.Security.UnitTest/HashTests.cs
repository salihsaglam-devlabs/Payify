using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace LinkPara.Security.UnitTest;

using static Testing;

public class HashTests
{
    private const string Key = "TestingHasherKey";
    
    [Test]
    public void ShouldHashProperly()
    {
        using var scope = ScopeFactory.CreateScope();
        var hashGenerator = scope.ServiceProvider.GetRequiredService<IHashGenerator>();
        
        var text = "Small text";

        var hash1 = hashGenerator.Generate(text, Key);
        var hash2 = hashGenerator.Generate(text, Key);
        
        hash1.Should().Be(hash2);
    }
}