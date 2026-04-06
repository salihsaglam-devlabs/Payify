using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountListQuery;

public class GetAccountListQuery : SearchQueryParams, IRequest<PaginatedList<AccountDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string IdentityNumber { get; set; }
    public string WalletNumber { get; set; }
    public AccountType? AccountType { get; set; }
    public AccountKycLevel? AccountKycLevel { get; set; }
    public AccountStatus? AccountStatus { get; set; }
    public bool? IsCommercial { get; set; }
}

public class GetAccountListQueryHandler : IRequestHandler<GetAccountListQuery, PaginatedList<AccountDto>>
{
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IMapper _mapper;

    public GetAccountListQueryHandler(IGenericRepository<Account> accountRepository,
        IMapper mapper)
    {
        _accountRepository = accountRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AccountDto>> Handle(GetAccountListQuery request, CancellationToken cancellationToken)
    {
        var query = _accountRepository.GetAll()
            .Include(s => s.Wallets)
            .AsQueryable();

        query = ApplyDateFilters(request, query);

        query = ApplyStringFilters(request, query);

        query = ApplyEnumFilters(request, query);

        query = ApplyBooleanFilters(request, query);

        return await query
            .PaginatedListWithMappingAsync<Account,AccountDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    private static IQueryable<Account> ApplyDateFilters(GetAccountListQuery request, IQueryable<Account> query)
    {
        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreateDate >= request.StartDate);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreateDate <= request.EndDate);
        }

        return query;
    }

    private static IQueryable<Account> ApplyEnumFilters(GetAccountListQuery request, IQueryable<Account> query)
    {
        if (request.AccountType.HasValue)
        {
            query = query.Where(s => s.AccountType == request.AccountType.Value);
        }

        if (request.AccountKycLevel.HasValue)
        {
            query = query.Where(s => s.AccountKycLevel == request.AccountKycLevel.Value);
        }

        if (request.AccountStatus.HasValue)
        {
            query = query.Where(s => s.AccountStatus == request.AccountStatus.Value);
        }

        return query;
    }

    private static IQueryable<Account> ApplyStringFilters(GetAccountListQuery request, IQueryable<Account> query)
    {
        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(s => s.Name.ToLower().Contains(request.Name.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.Email))
        {
            query = query.Where(s => s.Email.ToLower().Contains(request.Email.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            query = query.Where(s => (s.PhoneCode + s.PhoneNumber).Contains(request.PhoneNumber));
        }

        if (!string.IsNullOrEmpty(request.IdentityNumber))
        {
            query = query.Where(s => s.IdentityNumber == request.IdentityNumber);
        }
        if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            query = query.Where(s => s.Wallets.Any(a=>a.WalletNumber==request.WalletNumber));
        }

        return query;
    }
    
    private static IQueryable<Account> ApplyBooleanFilters(GetAccountListQuery request, IQueryable<Account> query)
    {
        if (request.IsCommercial.HasValue)
        {
            query = query.Where(s => s.IsCommercial == request.IsCommercial);
        }
        
        return query;
    }
}
