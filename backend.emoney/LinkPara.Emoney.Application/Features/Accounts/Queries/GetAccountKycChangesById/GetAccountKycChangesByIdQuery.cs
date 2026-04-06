using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountKycChangesById;

public class GetAccountKycChangesByIdQuery : IRequest<List<AccountKycChangeDto>>
{
    public Guid Id { get; set; }
}

public class GetAccountKycChangesByIdQueryHandler : IRequestHandler<GetAccountKycChangesByIdQuery, List<AccountKycChangeDto>>
{
    private readonly IGenericRepository<AccountKycChange> _repository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IMapper _mapper;

    public GetAccountKycChangesByIdQueryHandler(
        IGenericRepository<AccountKycChange> repository, 
        IGenericRepository<Account> accountRepository, 
        IMapper mapper)
    {
        _repository = repository;
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<List<AccountKycChangeDto>> Handle(GetAccountKycChangesByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken: cancellationToken);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), request.Id);
        }

        var kycChanges = await _repository
            .GetAll()
            .Where(k => k.AccountId == request.Id)
            .OrderByDescending(k => k.CreateDate)
            .ToListAsync(cancellationToken: cancellationToken);
        
        return _mapper.Map<List<AccountKycChangeDto>>(kycChanges);
    }
}
