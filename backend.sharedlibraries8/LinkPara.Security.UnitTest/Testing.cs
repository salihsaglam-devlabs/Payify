using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace LinkPara.Security.UnitTest;

[SetUpFixture]
public class Testing
{
    public static IServiceScopeFactory ScopeFactory;
    
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        services.AddScoped<IHashGenerator, HashGenerator>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<ISecureKeyGenerator, SecureKeyGenerator>();
        
        ScopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }
}