using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LinkPara.ContextProvider;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedBankAccountById;

public class GetSavedBankAccountByIdQuery : IRequest<SavedBankAccountDto>
{
    public Guid Id { get; set; }
}

public class GetSavedBankAccountByIdQueryHandler : IRequestHandler<GetSavedBankAccountByIdQuery, SavedBankAccountDto>
{
    private readonly IGenericRepository<SavedBankAccount> _repository;
    private readonly IGenericRepository<BankLogo> _bankLogoRepository;
    private readonly IMapper _mapper;
    private readonly IContextProvider _contextProvider;

    public GetSavedBankAccountByIdQueryHandler(IMapper mapper,
        IGenericRepository<SavedBankAccount> repository,
        IGenericRepository<BankLogo> bankLogoRepository,
        IContextProvider contextProvider)
    {
        _mapper = mapper;
        _repository = repository;
        _bankLogoRepository = bankLogoRepository;
        _contextProvider = contextProvider;
    }

    public async Task<SavedBankAccountDto> Handle(GetSavedBankAccountByIdQuery query, CancellationToken cancellationToken)
    {
        var bankLogos = _bankLogoRepository.GetAll();

        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        var bank = await _repository.GetAll()
            .Where(x =>
                x.Id == query.Id &&
                x.RecordStatus == RecordStatus.Active &&
                x.UserId == loggedUser)
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
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (bank is null)
        {
            throw new NotFoundException(nameof(bank), query.Id);
        }

        return _mapper.Map<SavedBankAccountDto>(bank);
    }
}
