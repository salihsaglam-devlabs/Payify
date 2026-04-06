using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountDetailQuery;

public class GetAccountDetailQuery : IRequest<AccountDto>
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
}

public class GetAccountDetailQueryHandler : IRequestHandler<GetAccountDetailQuery, AccountDto>
{
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<Account> _accountRepository;

    public GetAccountDetailQueryHandler(
        IGenericRepository<AccountUser> accountUserRepository,
        IMapper mapper,
        IGenericRepository<Wallet> walletRepository,
        IGenericRepository<Account> accountRepository)
    {
        _accountUserRepository = accountUserRepository;
        _mapper = mapper;
        _walletRepository = walletRepository;
        _accountRepository = accountRepository;
    }

    public async Task<AccountDto> Handle(GetAccountDetailQuery request, CancellationToken cancellationToken)
    {
        Account account = null;

        if (request.UserId != Guid.Empty)
        {
            var accountUser = await _accountUserRepository
                .GetAll()
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.UserId == request.UserId);

            if (accountUser is null)
            {
                throw new NotFoundException(nameof(AccountUser), request.UserId);
            }

            if (accountUser.Account is null)
            {
                throw new NotFoundException(nameof(Account), accountUser.AccountId);
            }

            account = accountUser.Account;
        }
        else if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            var wallet = await _walletRepository
                .GetAll()
                .FirstOrDefaultAsync(s => s.WalletNumber == request.WalletNumber);

            if (wallet is null)
            {
                throw new NotFoundException(nameof(Wallet), request.WalletNumber);
            }

            account = await _accountRepository.GetByIdAsync(wallet.AccountId);

            if (account is null)
            {
                throw new NotFoundException(nameof(Account), wallet.AccountId);
            }

        }

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), request.WalletNumber);
        }
        
        if (account.RecordStatus == RecordStatus.Passive 
            && !string.IsNullOrEmpty(account.IdentityNumber))
        {
            Regex identityNumber = new Regex(@"(?<!\d)\d{11}(?!\d)");
            var value = identityNumber.Match(account.IdentityNumber);
            account.IdentityNumber = value.Success ? value.Value : account.IdentityNumber;
        }
        return _mapper.Map<AccountDto>(account);
    }
}
