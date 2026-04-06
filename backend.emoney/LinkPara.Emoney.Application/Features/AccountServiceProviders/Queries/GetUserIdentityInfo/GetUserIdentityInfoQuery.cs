using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.GetUserIdentityInfo;

public class GetUserIdentityInfoQuery : IRequest<IdentityInfoDto>
{
    public string xAppUserId { get; set; }
}

public class GetUserIdentityInfoQueryHandler : IRequestHandler<GetUserIdentityInfoQuery, IdentityInfoDto>
{
    private readonly IGenericRepository<Account> _accountRepository;

    public GetUserIdentityInfoQueryHandler(IGenericRepository<Account> accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<IdentityInfoDto> Handle(GetUserIdentityInfoQuery request, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(request.xAppUserId))
        {
            throw new NotFoundException(nameof(Account), request.xAppUserId);
        }

        var account = await _accountRepository
            .GetAll()
            .Where(x => x.IdentityNumber == request.xAppUserId
                     && x.RecordStatus == RecordStatus.Active
                     && x.AccountType == AccountType.Individual)
            .Select(x => new IdentityInfoDto
            {
                OhkTur = "B",
                KmlkTur = "K",
                KmlkVrs = x.IdentityNumber,
                Unv = x.Name
            })
            .FirstOrDefaultAsync();

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), request.xAppUserId);
        }

        return account;

    }
}