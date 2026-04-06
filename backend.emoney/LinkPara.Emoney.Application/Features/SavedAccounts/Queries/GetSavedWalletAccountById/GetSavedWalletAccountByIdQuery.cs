using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LinkPara.ContextProvider;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedWalletAccountById;

public class GetSavedWalletAccountByIdQuery : IRequest<SavedWalletAccountDto>
{
    public Guid Id { get; set; }
}

public class GetSavedWalletByIdQueryHandler : IRequestHandler<GetSavedWalletAccountByIdQuery, SavedWalletAccountDto>
{
    private readonly IGenericRepository<SavedWalletAccount> _repository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IContextProvider _contextProvider;

    public GetSavedWalletByIdQueryHandler(
        IGenericRepository<SavedWalletAccount> repository,
        IMapper mapper,
        IGenericRepository<Account> accountRepository,
        IContextProvider contextProvider)
    {
        _repository = repository;
        _mapper = mapper;
        _accountRepository = accountRepository;
        _contextProvider = contextProvider;
    }

    public async Task<SavedWalletAccountDto> Handle(GetSavedWalletAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        var savedWallet = await _repository.GetAll()
                     .Where(x =>
                        x.Id == request.Id &&
                        x.RecordStatus == RecordStatus.Active &&
                        x.UserId == loggedUser)
                     .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (savedWallet is null)
        {
            throw new NotFoundException(nameof(savedWallet), request.Id);
        }

        var account = await _accountRepository.GetByIdAsync(savedWallet.WalletOwnerAccountId);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), savedWallet.WalletOwnerAccountId);
        }

        var savedAccount = _mapper.Map<SavedWalletAccountDto>(savedWallet);
        savedAccount.WalletOwnerName = account.Name;

        return savedAccount;
    }
}