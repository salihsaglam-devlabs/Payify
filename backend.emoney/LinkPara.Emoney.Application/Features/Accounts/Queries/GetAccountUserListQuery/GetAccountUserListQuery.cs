using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountUserListQuery;

public class GetAccountUserListQuery : IRequest<List<AccountUserDto>>
{
    public Guid AccountId { get; set; }
}

public class GetAccountUserListQueryHandler : IRequestHandler<GetAccountUserListQuery, List<AccountUserDto>>
{
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IMapper _mapper;

    public GetAccountUserListQueryHandler(
        IGenericRepository<Account> accountRepository, 
        IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<List<AccountUserDto>> Handle(GetAccountUserListQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository
            .GetAll()
            .Include(s => s.AccountUsers)
            .SingleOrDefaultAsync(s => s.Id == request.AccountId);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        return _mapper.Map<List<AccountUserDto>>(account.AccountUsers);
    }
}
