using FluentAssertions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Provisions.Commands.Provision;
using LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace LinkPara.Emoney.IntegrationTests.Provisions.Commands;

using static Testing;

public class ProvisionTests
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        var command = new TransferCommand();
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldProvisionMoneyAsync()
    {
        var amount = 100;
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var sourceWallet =
            context.Wallet.FirstOrDefault(w => w.CurrentBalanceCash > amount);

        if (sourceWallet is null)
        {
            throw new NotFoundException(nameof(Wallet));
        }
        
        var availableBalance = sourceWallet.AvailableBalance;
        
        var provisionService = scope.ServiceProvider.GetRequiredService<IProvisionService>();
        
        await provisionService.ProvisionAsync(new ProvisionCommand
        {
            Amount = amount,
            Description = "Unit Test Provision",
            ConversationId = Guid.NewGuid().ToString(),
            CurrencyCode = "TRY",
            ProvisionSource = ProvisionSource.Billing,
            UserId = Guid.NewGuid(),
            WalletNumber = sourceWallet.WalletNumber,
            ClientIpAddress = "192.1.1.1"
        }, new CancellationToken());
        
        var sourceWalletAfterProvision = 
            context.Wallet.FirstOrDefault(w => w.WalletNumber == sourceWallet.WalletNumber);
        
        sourceWalletAfterProvision!.LastActivityDate
            .Should()
            .BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
        sourceWalletAfterProvision!.AvailableBalance.Should().Be(availableBalance - amount);
    }
}