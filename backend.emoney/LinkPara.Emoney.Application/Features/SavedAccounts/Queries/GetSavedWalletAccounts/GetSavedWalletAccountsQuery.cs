using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedWalletAccounts;

public class GetSavedWalletAccountsQuery : IRequest<List<SavedWalletAccountDto>>
{
    public Guid UserId { get; set; }
}

public class GetSavedWalletAccountsQueryHandler : IRequestHandler<GetSavedWalletAccountsQuery, List<SavedWalletAccountDto>>
{
    private readonly IGenericRepository<SavedWalletAccount> _repository;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IMapper _mapper;
    private readonly IContextProvider _contextProvider;

    public GetSavedWalletAccountsQueryHandler(
        IGenericRepository<SavedWalletAccount> repository,
        IMapper mapper,
        IGenericRepository<AccountUser> accountUserRepository,
        IContextProvider contextProvider)
    {
        _repository = repository;
        _mapper = mapper;
        _accountUserRepository = accountUserRepository;
        _contextProvider = contextProvider;
    }

    public async Task<List<SavedWalletAccountDto>> Handle(GetSavedWalletAccountsQuery request, CancellationToken cancellationToken)
    {
        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        if (request.UserId != loggedUser)
        {
            throw new ForbiddenAccessException();
        }

        var wallets = await _repository.GetAll()
            .Where(x => x.UserId == request.UserId)
            .Where(x => x.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken: cancellationToken);

        var list = new List<SavedWalletAccountDto>();

        foreach (var item in wallets)
        {
            var accountUser = await _accountUserRepository.GetAll()
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s => s.UserId == request.UserId);

            if (accountUser is null)
            {
                throw new NotFoundException(nameof(AccountUser), request.UserId);
            }

            var savedAccount = _mapper.Map<SavedWalletAccountDto>(item);
            savedAccount.WalletOwnerName = accountUser.Account.Name;
            list.Add(savedAccount);
        }

        return list;
    }
}