using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Approval.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Approval.Application.Features.Cases.Queries.GetCases;

public class GetCasesQuery : SearchQueryParams, IRequest<PaginatedList<CaseDto>>
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public string ModuleName { get; set; }
}

public class GetCasesQueryHandler : IRequestHandler<GetCasesQuery, PaginatedList<CaseDto>>
{

    private readonly IGenericRepository<Case> _repository;
    private readonly IMapper _mapper;

    public GetCasesQueryHandler(IGenericRepository<Case> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<CaseDto>> Handle(GetCasesQuery request, CancellationToken cancellationToken)
    {
        var cases = _repository.GetAll();

        if (!string.IsNullOrEmpty(request.Q))
        {
            cases = cases.Where(b => b.DisplayName.ToLower().Contains(request.Q.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.ModuleName))
        {
            cases = cases.Where(b => b.ModuleName.Contains(request.ModuleName));
        }

        if (request.CreateDateStart is not null)
        {
            cases = cases.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            cases = cases.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.RecordStatus is not null)
        {
            cases = cases.Where(b => b.RecordStatus
                               == request.RecordStatus);
        }

        return await cases.ProjectTo<CaseDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
