using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Infrastructure.ExternalServices.Banking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LinkPara.Emoney.Test;

public class VakifKatilimTest
{
    [Fact]
    public async Task GetCityListShouldListNotEmpty()
    {       

        var response = await new
            VakifKatilimService(new VakifKatilimBankService.CollectionManagementServiceClient(), getConfig()
           )
            .GetBankListAsync();

        Assert.True(response.Success);
    }

    public static IConfiguration getConfig()
    {
        var config = new ConfigurationBuilder()
          .SetBasePath(
            Directory
            .GetParent(Directory.GetCurrentDirectory())          
            .Parent
            .Parent
            .Parent + "\\LinkPara.Emoney.API\\")
          .AddJsonFile("appsettings.json")
          .Build();
        return config;
    }
}