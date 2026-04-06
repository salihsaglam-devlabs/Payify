using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CompanyPools.Queries.GetCompanyPoolList;

public class GetCompanyPoolListQuery : SearchQueryParams,IRequest<PaginatedList<CompanyPoolDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Title { get; set; }
    public CompanyPoolStatus? CompanyPoolStatus { get; set; }
    public CompanyPoolChannel? Channel { get; set; }
    public CompanyType? CompanyType { get; set; }
}

public class GetCompanyPoolListQueryHandler : IRequestHandler<GetCompanyPoolListQuery, PaginatedList<CompanyPoolDto>>
{
    private readonly IGenericRepository<CompanyPool> _repository;
    private readonly IMapper _mapper;

    public GetCompanyPoolListQueryHandler(IGenericRepository<CompanyPool> repository, 
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<CompanyPoolDto>> Handle(GetCompanyPoolListQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetAll()
           .AsQueryable();

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreateDate >= request.StartDate);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreateDate <= request.EndDate);
        }

        if (request.CompanyPoolStatus.HasValue)
        {
            query = query.Where(s => s.CompanyPoolStatus == request.CompanyPoolStatus.Value);
        }

        if (request.CompanyType.HasValue)
        {
            query = query.Where(s => s.CompanyType == request.CompanyType.Value);
        }
        if (request.Channel.HasValue)
        {
            query = query.Where(s => s.Channel == request.Channel.Value);
        }

        if (!string.IsNullOrEmpty(request.Title))
        {
            query = query.Where(s => s.Title.ToLower().Contains(request.Title.ToLower()));
        }
     
        return await query
           .PaginatedListWithMappingAsync<CompanyPool, CompanyPoolDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
