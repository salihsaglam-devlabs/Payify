using LinkPara.Kkb.Application.Commons.Interfaces;
using LinkPara.Kkb.Infrastructure.ExternalServices.Kkb;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Kkb.IntegrationTests.KkbExternalServiceTests;

using static Testing;
public class AuthorizationServiceTests
{
    [Test]
    public async Task GetTokenTest()
    {
        var scope = _scopeFactory.CreateScope(); 
        var service = scope.ServiceProvider.GetService<IKkbAuthorizationService>();

        var token = await service.GetTokenAsync();

        Assert.That(token, Is.Not.Null);
    }

    [Test]
    public async Task GetTokenFromMemoryTest()
    {
        var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetService<IKkbAuthorizationService>();

        var token1 = await service.GetTokenAsync();
        var token2 = await service.GetTokenAsync();

        Assert.That(token2, Is.EqualTo(token1));
    }
}
