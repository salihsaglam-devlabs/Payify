using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetParentAccountByUserId;

public class GetParentAccountByUserIdQuery : IRequest<ParentAccountResponse>
{
    public Guid UserId { get; set; }
}

public class GetParentAccountByUserIdQueryHandler : IRequestHandler<GetParentAccountByUserIdQuery, ParentAccountResponse>
{
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IMapper _mapper;

    public GetParentAccountByUserIdQueryHandler(
        IGenericRepository<AccountUser> accountUserRepository,
        IGenericRepository<Account> accountRepository,
        IMapper mapper)
    {
        _accountUserRepository = accountUserRepository;
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<ParentAccountResponse> Handle(GetParentAccountByUserIdQuery request, CancellationToken cancellationToken)
    {
        var accountUser = await _accountUserRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.UserId == request.UserId
                && s.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

        if (accountUser is null)
        {
            return new ParentAccountResponse { IsExist = false };
        }

        var account = await _accountRepository
           .GetAll()
           .FirstOrDefaultAsync(s => s.Id == accountUser.AccountId
                && s.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

        if (account is null)
        {
            return new ParentAccountResponse { IsExist = false };
        }

        var parentAccount = await _accountRepository
           .GetAll()
           .FirstOrDefaultAsync(s => s.Id == account.ParentAccountId
                && s.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

        if (parentAccount is null)
        {
            return new ParentAccountResponse { IsExist = false };
        }

        var accountResponse =_mapper.Map<AccountDto>(parentAccount);
        return new ParentAccountResponse { IsExist = true , AccountDto = accountResponse };
    }
}

