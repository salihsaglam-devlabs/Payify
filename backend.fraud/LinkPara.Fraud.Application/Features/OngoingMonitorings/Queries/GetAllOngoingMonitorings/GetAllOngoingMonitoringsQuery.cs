using AutoMapper;
using LinkPara.Fraud.Domain.Entities;
using LinkPara.Fraud.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Fraud.Application.Features.OngoingMonitorings.Queries.GetAllOngoingMonitorings;

public class GetAllOngoingMonitoringsQuery : SearchQueryParams, IRequest<PaginatedList<OngoingMonitoringDto>>
{
    public string SearchName { get; set; }
    public SearchType? SearchType { get; set; }
    public OngoingPeriod? OngoingPeriod { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public bool? IsOngoingList { get; set; }
}
public class GetAllOngoingMonitoringsQueryHandler : IRequestHandler<GetAllOngoingMonitoringsQuery, PaginatedList<OngoingMonitoringDto>>
{
    private readonly IGenericRepository<OngoingMonitoring> _ongoingRepository;
    private readonly IMapper _mapper;

    public GetAllOngoingMonitoringsQueryHandler(IGenericRepository<OngoingMonitoring> ongoingRepository, IMapper mapper)
    {
        _ongoingRepository = ongoingRepository;
        _mapper = mapper;
    }
    public async Task<PaginatedList<OngoingMonitoringDto>> Handle(GetAllOngoingMonitoringsQuery request, CancellationToken cancellationToken)
    {
        var ongoingList = _ongoingRepository.GetAll();

        if (!string.IsNullOrEmpty(request.SearchName))
        {
            ongoingList = ongoingList.Where(b => b.SearchName.ToLower()
                                         .Contains(request.SearchName.ToLower()));
        }
        if (!string.IsNullOrEmpty(request.Q))
        {
            ongoingList = ongoingList.Where(b => b.ScanId.ToLower()
                                         .Contains(request.Q.ToLower()));
        }
        if (request.SearchType is not null)
        {
            ongoingList = ongoingList.Where(b => b.SearchType == request.SearchType);
        }
        if (request.OngoingPeriod is not null)
        {
            ongoingList = ongoingList.Where(b => b.Period == request.OngoingPeriod);
        }
        if (request.IsOngoingList is not null)
        {
            ongoingList = ongoingList.Where(b => b.IsOngoingList == request.IsOngoingList);
        }
        if (request.DateStart is not null)
        {
            ongoingList = ongoingList.Where(b => b.CreateDate
                               >= request.DateStart);
        }
        if (request.DateEnd is not null)
        {
            ongoingList = ongoingList.Where(b => b.CreateDate
                                                     <= request.DateEnd);
        }
        return await ongoingList.PaginatedListWithMappingAsync<OngoingMonitoring, OngoingMonitoringDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}