using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetCorporateAccountList;

public class GetCorporateAccountListQuery : SearchQueryParams, IRequest<PaginatedList<CorporateAccountDto>>
{
    public string Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public CompanyType? CompanyType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class GetCorporateAccountListQueryHandler : IRequestHandler<GetCorporateAccountListQuery, PaginatedList<CorporateAccountDto>>
{
    private readonly IGenericRepository<Account> _repository;
    private readonly IMapper _mapper;
    public GetCorporateAccountListQueryHandler(IGenericRepository<Account> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<CorporateAccountDto>> Handle(GetCorporateAccountListQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetAll()
            .Where(x => x.AccountType == AccountType.Corporate)
            .Include(x=>x.CompanyPool).AsQueryable();

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreateDate >= request.StartDate);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreateDate <= request.EndDate);
        }

        if (request.CompanyType.HasValue)
        {
            query = query.Where(s => s.CompanyPool.CompanyType == request.CompanyType.Value);
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(s => s.Name.ToLower().Contains(request.Name.ToLower()));
        }

        if (request.RecordStatus.HasValue)
        {
            query = query.Where(s => s.RecordStatus == request.RecordStatus);
        }

        return await query
         .PaginatedListWithMappingAsync<Account, CorporateAccountDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

    }
}
