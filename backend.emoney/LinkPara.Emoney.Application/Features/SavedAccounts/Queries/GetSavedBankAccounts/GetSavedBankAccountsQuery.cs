using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedBankAccounts;

public class GetSavedBankAccountsQuery : IRequest<List<SavedBankAccountDto>>
{
    public Guid UserId { get; set; }
}

public class GetSavedBankAccountsQueryHandler : IRequestHandler<GetSavedBankAccountsQuery, List<SavedBankAccountDto>>
{
    private readonly IGenericRepository<SavedBankAccount> _repository;
    private readonly IGenericRepository<BankLogo> _bankLogoRepository;
    private readonly IMapper _mapper;
    private readonly IContextProvider _contextProvider;

    public GetSavedBankAccountsQueryHandler(IMapper mapper,
        IGenericRepository<SavedBankAccount> repository,
        IGenericRepository<BankLogo> bankLogoRepository,
        IContextProvider contextProvider)
    {
        _mapper = mapper;
        _repository = repository;
        _bankLogoRepository = bankLogoRepository;
        _contextProvider = contextProvider;
    }

    public async Task<List<SavedBankAccountDto>> Handle(GetSavedBankAccountsQuery query, CancellationToken cancellationToken)
    {
        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        if (query.UserId != loggedUser)
        {
            throw new ForbiddenAccessException();
        }

        var bankLogos = _bankLogoRepository.GetAll();

        var banks = await _repository.GetAll()
            .Where(x =>
                x.UserId == query.UserId &&
                x.RecordStatus == RecordStatus.Active)
            .Include(x => x.Bank)
            .Select(x => new SavedBankAccount
            {
                Id = x.Id,
                BankId = x.BankId,
                Bank = new Bank
                {
                    Code = x.Bank.Code,
                    Name = x.Bank.Name,
                    HasLogo = bankLogos.Any(q => q.BankId == x.BankId)
                },
                CreateDate = x.CreateDate,
                CreatedBy = x.CreatedBy,
                Iban = x.Iban,
                LastModifiedBy = x.LastModifiedBy,
                Tag = x.Tag,
                Type = x.Type,
                UpdateDate = x.UpdateDate,
                RecordStatus = x.RecordStatus,
                UserId = x.UserId,
                ReceiverName = x.ReceiverName
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return _mapper.Map<List<SavedBankAccountDto>>(banks);
    }
}