using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetApprovalScreenWalletList;

public class GetApprovalScreenWalletListQuery : IRequest<List<ApprovalScreenWalletDto>>
{
    public string AccountNumber { get; set; }
}

public class GetWalletListQueryHandler : IRequestHandler<GetApprovalScreenWalletListQuery, List<ApprovalScreenWalletDto>>
{
    private readonly IGenericRepository<Account> _accountRepository;

    public GetWalletListQueryHandler(IGenericRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<List<ApprovalScreenWalletDto>> Handle(GetApprovalScreenWalletListQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.AccountNumber, out var accountId))
        {
            throw new InvalidCastException();
        }
        var account = await _accountRepository
            .GetAll()
            .Include(x => x.Wallets.Where(w => w.RecordStatus == RecordStatus.Active))
            .Where(x => x.Id == accountId && x.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync();

        if (account is null)
        {
            throw new NotFoundException(nameof(Account),request.AccountNumber);
        }

        var accountWallets = account.Wallets.Select(x => new ApprovalScreenWalletDto
        {
            Currency = x.CurrencyCode,
            CustomerName = account.Name,
            ProductName = "VADESIZ",
            ReferenceNumber = x.WalletNumber,
            Iban = x.WalletNumber
        }).ToList();

        return accountWallets;
    }
}