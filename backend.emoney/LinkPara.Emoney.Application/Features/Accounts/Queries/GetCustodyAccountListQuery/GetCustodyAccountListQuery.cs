using AutoMapper;
using LinkPara.Emoney.Application.Commons.Models.AccountModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetCustodyAccountListQuery;

public class GetCustodyAccountListQuery : SearchQueryParams, IRequest<PaginatedList<CustodyAccountResponse>>
{
    public string ParentAccountId { get; set; }
    public string ParentIdentityNumber { get; set; }
    public string ParentNameSurname { get; set; }
    public string ParentPhoneNumber { get; set; }
    public string ChildIdentityNumber { get; set; }
    public string ChildNameSurname { get; set; }
    public string ChildPhoneNumber { get; set; }
}

public class GetCustodyAccountListQueryHandler : IRequestHandler<GetCustodyAccountListQuery, PaginatedList<CustodyAccountResponse>>
{    
    private readonly IGenericRepository<Account> _accountRepository;

    public GetCustodyAccountListQueryHandler(
        IGenericRepository<Account> accountRepository)
    {       
        _accountRepository = accountRepository;
    }

    public async Task<PaginatedList<CustodyAccountResponse>> Handle(GetCustodyAccountListQuery request, CancellationToken cancellationToken)
    {
        var accounts = _accountRepository.GetAll();
        var childAccounts = _accountRepository
            .GetAll()
            .Where(s => s.ParentAccountId != Guid.Empty && s.RecordStatus == RecordStatus.Active);


        var list = childAccounts
            .Join(accounts,
            f => f.ParentAccountId, s => s.Id, (f, s) => new CustodyAccountResponse
            {
                AccountId = f.Id,
                ParentAccountId = s.Id,
                ParentIdentityNumber = s.IdentityNumber,
                ParentNameSurname = s.Name,
                ParentPhoneNumber = s.PhoneNumber,
                ParentEmail = s.Email,
                ChildIdentityNumber = f.IdentityNumber,
                ChildNameSurname = f.Name,
                ChildPhoneNumber = f.PhoneNumber,
                ChildEmail = f.Email
            });

        list = ApplyParentFilters(request, list);
        list = ApplyChildFilters(request, list);
       
        return await list.PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    private static IQueryable<CustodyAccountResponse> ApplyParentFilters(GetCustodyAccountListQuery request, IQueryable<CustodyAccountResponse> query)
    {
        if (!string.IsNullOrEmpty(request.ParentAccountId))
        {
            query = query.Where(s => s.ParentAccountId == new Guid(request.ParentAccountId));
        }

        if (!string.IsNullOrEmpty(request.ParentNameSurname))
        {
            query = query.Where(s => s.ParentNameSurname.ToLower().Contains(request.ParentNameSurname.ToLower()));
        }
        
        if (!string.IsNullOrEmpty(request.ParentPhoneNumber))
        {
            query = query.Where(s => (s.ParentPhoneNumber).Contains(request.ParentPhoneNumber));
        }

        if (!string.IsNullOrEmpty(request.ParentIdentityNumber))
        {
            query = query.Where(s => (s.ParentIdentityNumber).Contains(request.ParentIdentityNumber));
        }        

        return query;
    }

    private static IQueryable<CustodyAccountResponse> ApplyChildFilters(GetCustodyAccountListQuery request, IQueryable<CustodyAccountResponse> query)
    {
        if (!string.IsNullOrEmpty(request.ChildNameSurname))
        {
            query = query.Where(s => s.ChildNameSurname.ToLower().Contains(request.ChildNameSurname.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.ChildPhoneNumber))
        {
            query = query.Where(s => (s.ChildPhoneNumber).Contains(request.ChildPhoneNumber));
        }

        if (!string.IsNullOrEmpty(request.ChildIdentityNumber))
        {
            query = query.Where(s => (s.ChildIdentityNumber).Contains(request.ChildIdentityNumber));
        }

        return query;
    }
}
