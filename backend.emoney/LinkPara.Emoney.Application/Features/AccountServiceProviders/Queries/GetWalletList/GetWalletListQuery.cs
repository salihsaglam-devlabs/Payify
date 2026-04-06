using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetWalletList;

public class GetWalletListQuery : IRequest<List<CustomerWalletDto>>
{
    public Guid AccountId { get; set; }
    public string AccountRef { get; set; }
}

public class GetWalletListQueryHandler : IRequestHandler<GetWalletListQuery, List<CustomerWalletDto>>
{
    private readonly IGenericRepository<Account> _accountRepository;

    public GetWalletListQueryHandler(IGenericRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<List<CustomerWalletDto>> Handle(GetWalletListQuery request, CancellationToken cancellationToken)
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

        var result = new List<CustomerWalletDto>();

        if (account.Wallets.Any())
        {
            foreach (var wallet in account.Wallets)
            {
                var walletDto = new CustomerWalletDto
                {
                    HspAclsTrh = wallet.OpeningDate?.ToString("yyyy-MM-dd’T’HH: mm:ssXXX"),
                    HspDrm = "AKTIF",
                    HspNo = wallet.WalletNumber,
                    HspRef = wallet.WalletNumber,
                    HspShb = account.Name,
                    HspTip = "VADESIZ",
                    HspTur = wallet.WalletType == WalletType.Individual ? "B" : "T",
                    KisaAd = wallet.FriendlyName,
                    PrBrm = wallet.CurrencyCode
                };
                result.Add(walletDto);
            }
        }

        return result;
    }
}
