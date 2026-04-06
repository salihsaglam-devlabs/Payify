using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;
using LinkPara.Approval.Models.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.BusModels.Commands.BTrans.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.Requests.Queries.GetRequests;

public class GetCashbackRequestsQuery : SearchQueryParams, IRequest<PaginatedList<RequestCashbackDto>>
{
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public ActionType? ActionType { get; set; }
    public RequestOperationType? OperationType { get; set; }
    public List<ApprovalStatus>? Statuses { get; set; }
}

public class GetCashbackRequestsQueryHandler : IRequestHandler<GetCashbackRequestsQuery, PaginatedList<RequestCashbackDto>>
{
    private const string CashbackResourceIdentifier = "Cashback";
    private readonly IGenericRepository<Request> _requestRepository;
    private readonly IMapper _mapper;

    public GetCashbackRequestsQueryHandler(IGenericRepository<Request> requestRepository,
        IMapper mapper)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<RequestCashbackDto>> Handle(GetCashbackRequestsQuery request, CancellationToken cancellationToken)
    {
        var cashbackRequests = _requestRepository.GetAll()
            .Where(x => x.Resource.Contains(CashbackResourceIdentifier));

        if (request.Statuses is not null && request.Statuses.Any())
        {
            cashbackRequests = cashbackRequests.Where(s => request.Statuses.Contains(s.Status));
        }
        if (request.OperationType is not null)
        {
            cashbackRequests = cashbackRequests.Where(s => s.OperationType == request.OperationType);
        }
        if (request.ActionType is not null)
        {
            cashbackRequests = cashbackRequests.Where(s => s.ActionType == request.ActionType);
        }

        if (request.DateStart is not null)
        {
            cashbackRequests = cashbackRequests.Where(s => s.CreateDate >= request.DateStart);
        }

        if (request.DateEnd is not null)
        {
            cashbackRequests = cashbackRequests.Where(s => s.CreateDate <= request.DateEnd);
        }

        return await cashbackRequests
            .ProjectTo<RequestCashbackDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, string.IsNullOrEmpty(request.SortBy) ? "CreateDate" : request.SortBy);
    }
}
