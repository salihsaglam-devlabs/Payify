using FluentAssertions;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using NUnit.Framework;

namespace LinkPara.Emoney.IntegrationTests.Wallets.Commands;

using static Testing;

public class TransferCommandTest 
{
    // Test wallets
    private const string SourceWalletId = "2839e016-2121-4c15-b8e5-08da0d7ecf9b";
    private const string DestinationWalletId = "e6051b0c-390c-4ca3-81b4-7a2d365d9a6b";
    private const string SourceWalletNumber = "6706485315";
    private const string DestinationWalletNumber = "5589301374";

    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        var command = new TransferCommand();
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldTransferMoney()
    {
        var walletBeforeTransfer = await FindAsync<Wallet>(Guid.Parse(SourceWalletId));

        var initialCash = walletBeforeTransfer.CurrentBalanceCash;
        
        var commandDeposit = new TransferCommand
        {
            Amount = 10,
            Description = "Integration test transfer",
            SenderWalletNumber = SourceWalletNumber,
            ReceiverWalletNumber = DestinationWalletNumber,
            UserId = Guid.NewGuid().ToString()
        };

        await SendAsync(commandDeposit);
        
        var commandWithdraw = new TransferCommand
        {
            Amount = 10,
            Description = "Integration test transfer - rollback",
            SenderWalletNumber = DestinationWalletNumber,
            ReceiverWalletNumber = SourceWalletNumber,
            UserId = Guid.NewGuid().ToString()
        };
        
        await SendAsync(commandWithdraw);
        
        var wallet = await FindAsync<Wallet>(Guid.Parse(SourceWalletId));
        var receiverWallet = await FindAsync<Wallet>(Guid.Parse(DestinationWalletId));
        
        wallet.LastActivityDate.Should()
            .BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
        wallet.CurrentBalanceCash.Should().Be(initialCash);
        receiverWallet.LastActivityDate.Should()
            .BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }

    [Test]
    public async Task ShouldThrowInsufficientBalanceException()
    {
        var walletBeforeTransfer = await FindAsync<Wallet>(Guid.Parse(SourceWalletId));

        var initialCash = walletBeforeTransfer.CurrentBalanceCash;
        
        var commandDeposit = new TransferCommand
        {
            Amount = initialCash + 100,
            Description = "Integration test transfer",
            SenderWalletNumber = SourceWalletNumber,
            ReceiverWalletNumber = DestinationWalletNumber,
            UserId = Guid.NewGuid().ToString()
        };

        await FluentActions.Invoking(() => SendAsync(commandDeposit))
            .Should()
            .ThrowAsync<InsufficientBalanceException>();
    }
}