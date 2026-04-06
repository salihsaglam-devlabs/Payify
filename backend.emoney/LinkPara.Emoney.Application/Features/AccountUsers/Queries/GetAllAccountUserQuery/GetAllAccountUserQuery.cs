using AutoMapper;
using LinkPara.Emoney.Application.Features.Accounts;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using LinkPara.MappingExtensions.Mapping;
using MediatR;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Features.AccountUsers.Queries.GetAllAccountUserQuery;

public class GetAllAccountUserQuery : SearchQueryParams, IRequest<PaginatedList<AccountUserDto>>
{
    public string Fullname { get; set; }
    public Guid? AccountId { get; set; }
}

public class GetAllAccountUserQueryHandler : IRequestHandler<GetAllAccountUserQuery, PaginatedList<AccountUserDto>>
{
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IMapper _mapper;

    public GetAllAccountUserQueryHandler(
        IGenericRepository<AccountUser> accountUserRepository,
        IMapper mapper)
    {
        _accountUserRepository = accountUserRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AccountUserDto>> Handle(GetAllAccountUserQuery request, CancellationToken cancellationToken)
    {
        var accountUsers = _accountUserRepository.GetAll();

        if (!string.IsNullOrEmpty(request.Fullname))
        {
            accountUsers = accountUsers.Where(b =>
                                       (b.Firstname.ToLower() + " " + b.Lastname.ToLower())
                                       .Contains(request.Fullname.ToLower()));
        }

        if (request.AccountId is not null)
        {
            accountUsers = accountUsers.Where(b => b.AccountId == request.AccountId);
        }

        return await accountUsers
                    .PaginatedListWithMappingAsync<AccountUser, AccountUserDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy); ;
    }
}
