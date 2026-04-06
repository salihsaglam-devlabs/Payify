using AutoMapper;
using LinkPara.Emoney.Application.Features.Wallets;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountWalletListQuery;

public class GetAccountWalletListQuery : IRequest<List<WalletDto>>
{
    public Guid AccountId { get; set; }
}

public class GetAccountWalletListQueryHandler : IRequestHandler<GetAccountWalletListQuery, List<WalletDto>>
{
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IMapper _mapper;

    public GetAccountWalletListQueryHandler(IGenericRepository<Account> accountRepository,
        IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<List<WalletDto>> Handle(GetAccountWalletListQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository
            .GetAll()
            .Include(s => s.Wallets)
            .ThenInclude(s=>s.Currency)
            .SingleOrDefaultAsync(s => s.Id == request.AccountId);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        return _mapper.Map<List<WalletDto>>(account.Wallets);
    }
}

