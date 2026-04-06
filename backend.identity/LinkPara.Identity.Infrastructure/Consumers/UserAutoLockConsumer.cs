using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Models.IdentityConfiguration;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Identity;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Infrastructure.Consumers;

public class UserAutoLockConsumer : IConsumer<UserAutoLock>
{
    private readonly IVaultClient _vaultClient;
    private readonly UserManager<User> _userManager;

    public UserAutoLockConsumer(IVaultClient vaultClient,
        UserManager<User> userManager)
    {
        _vaultClient = vaultClient;
        _userManager = userManager;
    }

    public async Task Consume(ConsumeContext<UserAutoLock> context)
    {
        var user = await _userManager.FindByNameAsync(context.Message.Username);

        if (user is null)
        {
            return;
        }
        var lockoutSettings = _vaultClient.GetSecretValue<LockoutSettings>("IdentitySecrets", "LockoutSettings");

        var lockoutDateTimeOffset = new DateTimeOffset(DateTime.Now.AddMinutes(lockoutSettings.DefaultLockoutTimeSpanInMinutes));

        await _userManager.SetLockoutEndDateAsync(user, lockoutDateTimeOffset);

    }
}