using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetWalletBalanceList;

public class GetWalletBalanceListQuery : IRequest<List<CustomerWalletBalanceDto>>
{
    public Guid AccountId { get; set; }
    public string AccountRef { get; set; }
}


public class GetWalletBalanceListQueryHandler : IRequestHandler<GetWalletBalanceListQuery, List<CustomerWalletBalanceDto>>
{
    private readonly IGenericRepository<Account> _accountRepository;

    public GetWalletBalanceListQueryHandler(IGenericRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<List<CustomerWalletBalanceDto>> Handle(GetWalletBalanceListQuery request, CancellationToken cancellationToken)
    {

        var accountQuery = _accountRepository
            .GetAll()
            .Where(x => x.Id == request.AccountId
                             && x.RecordStatus == RecordStatus.Active).AsQueryable();

        if (String.IsNullOrEmpty(request.AccountRef))
        {
            accountQuery = accountQuery.Include(x => x.Wallets.Where(w => w.RecordStatus == RecordStatus.Active));
        }
        else
        {
            accountQuery = accountQuery.Include(x => x.Wallets.Where(w => w.RecordStatus == RecordStatus.Active && w.Id.ToString() == request.AccountRef));
        }

        var account = await accountQuery.FirstOrDefaultAsync();

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        var result = new List<CustomerWalletBalanceDto>();

        if (account.Wallets.Any())
        {
            foreach (var wallet in account.Wallets)
            {
                var walletDto = new CustomerWalletBalanceDto
                {
                    BkyZmn = DateTime.Now.ToString("yyyy-MM-dd’T’HH: mm:ssXXX"),
                    BkyTtr = wallet.CurrentBalanceCash.ToString(),
                    BlkTtr = wallet.BlockedBalance.ToString(),
                    HspRef = wallet.WalletNumber,
                    KulKrdTtr = wallet.CurrentBalanceCredit.ToString(),
                    KrdDhlGstr = "0",
                    PrBrm = wallet.CurrencyCode
                };
                result.Add(walletDto);
            }
        }

        return result;
    }
}
