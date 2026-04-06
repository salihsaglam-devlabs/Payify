using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Interfaces.Authorization;
using LinkPara.CampaignManagement.Application.Commons.IWalletConfigurations;
using LinkPara.CampaignManagement.Application.Commons.Models.EventBusConfiguration;
using LinkPara.CampaignManagement.Application.Commons.Models.LoginConfiguration;
using LinkPara.CampaignManagement.Application.Features.IWalletLogins;
using LinkPara.CampaignManagement.Application.Features.IWalletLogins.Commands.Login;
using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.Configuration;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class IWalletLoginService : IIWalletLoginService
{
    private readonly IJwtHelper _jwtHelper;
    private readonly IVaultClient _vaultClient;

    public IWalletLoginService(IJwtHelper jwtHelper,
        IVaultClient vaultClient)
    {
        _jwtHelper = jwtHelper;
        _vaultClient = vaultClient;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginCommand request)
    {
        var iWalletSettings = _vaultClient.GetSecretValue<IWalletSettings>("CampaignManagementSecrets", "IWalletSettings");

        var credentialSettings = iWalletSettings.LoginCredentials;

        if (credentialSettings is null)
        {
            throw new InvalidOperationException("CredentialsAreNotConfigure");
        }

        if (credentialSettings.Password != request.Password ||
           credentialSettings.Email != request.Email)
        {
            throw new UnauthorizedAccessException("EmailOrPasswordNotMatch");
        }

        var result = await _jwtHelper.GenerateJwtTokenAsync(credentialSettings.Id, TimeSpan.FromSeconds(credentialSettings.ExpiresInSecond));

        return result;
    }
}
