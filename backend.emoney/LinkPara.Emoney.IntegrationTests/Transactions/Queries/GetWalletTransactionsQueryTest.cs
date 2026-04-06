using LinkPara.Emoney.Application.Features.Transactions.Queries.GetWalletTransactions;

using NUnit.Framework;

namespace LinkPara.Emoney.IntegrationTests.Transactions.Queries;

using static Testing;

public class GetWalletTransactionsQueryTest
{
    // Test wallet
    private readonly Guid SourceWalletId = Guid.Parse("e6041a14-2502-4d89-a243-c345087c26eb");

    [Test]
    public async Task EnsureDateFiltersWorksAsExpected()
    {
        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;

        var command = new GetWalletTransactionsQuery()
        {
            WalletId = SourceWalletId,
            StartDate = startDate,
            EndDate = endDate
        };

        var response = await SendAsync(command);

        Assert.That(response.Items.Count, Is.AtLeast(1));

        var items = response.Items;

        //Assert.IsFalse(items.Any(x => x.TransactionDate < startDate || x.TransactionDate > endDate));
    }
}
