using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetUserAccountList;

public class GetUserAccountListQuery : IRequest<List<UserAccountResultDto>>
{
    public string xAppUserId { get; set; }
}

public class GetUserAccountListQueryHandler : IRequestHandler<GetUserAccountListQuery, List<UserAccountResultDto>>
{
    private readonly IGenericRepository<Account> _accountRepository;

    public GetUserAccountListQueryHandler(IGenericRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<List<UserAccountResultDto>> Handle(GetUserAccountListQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.xAppUserId))
        {
            throw new NotFoundException(nameof(Account), request.xAppUserId);
        }

        var account = await _accountRepository
            .GetAll()
            .Where(x => x.IdentityNumber == request.xAppUserId
                     && x.RecordStatus == RecordStatus.Active
                     && x.AccountType == AccountType.Individual)
            .Include(x => x.Wallets)
            .FirstOrDefaultAsync();

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), request.xAppUserId);
        }

        return account.Wallets.Select(x => new UserAccountResultDto
        {
            AccountName = x.FriendlyName,
            CustomerName = account.Name,
            Fec = x.CurrencyCode,
            IbanNo = x.WalletNumber,
            AvailableBalance = x.AvailableBalance.ToString()
        })
        .ToList();
    }
}