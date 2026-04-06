using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Approval.Application.Features.Requests.Queries.GetRequests;

public class GetRequestsQuery : SearchQueryParams, IRequest<PaginatedList<RequestDto>>
{
    public List<Guid> UserRoleIds { get; set; }
    public ApprovalStatus? ApprovalStatus { get; set; }
    public DateTime? UpdateDateStart { get; set; }
    public DateTime? UpdateDateEnd { get; set; }
    public RequestOperationType? OperationType { get; set; }
    public long? ReferenceId { get; set; }
}

public class GetRequestsQueryHandler : IRequestHandler<GetRequestsQuery, PaginatedList<RequestDto>>
{

    private readonly IGenericRepository<Request> _requestRepository;
    private readonly IMapper _mapper;

    public GetRequestsQueryHandler(IGenericRepository<Request> requestRepository,
        IMapper mapper)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<RequestDto>> Handle(GetRequestsQuery request, CancellationToken cancellationToken)
    {
        var pendingApprovals = _requestRepository.GetAll()
            .Where(x => x.Status != ApprovalStatus.Duplicated)
            .Where(x => request.UserRoleIds.Contains(x.CheckerRoleId) || request.UserRoleIds.Contains(x.SecondCheckerRoleId));

        if (!string.IsNullOrEmpty(request.Q))
        {
            pendingApprovals = pendingApprovals.Where(s => s.DisplayName.Contains(request.Q));
        }

        if (request.ApprovalStatus != null)
        {
            pendingApprovals = pendingApprovals.Where(s => s.Status == request.ApprovalStatus);
        }

        if (request.UpdateDateStart is not null)
        {
            pendingApprovals = pendingApprovals.Where(s => s.UpdateDate >= request.UpdateDateStart);
        }

        if (request.UpdateDateEnd is not null)
        {
            pendingApprovals = pendingApprovals.Where(s => s.UpdateDate <= request.UpdateDateEnd);
        }

        if (request.OperationType is not null)
        {
            pendingApprovals = pendingApprovals.Where(s => s.OperationType == request.OperationType);
        }

        if (request.ReferenceId is not null)
        {
            pendingApprovals = pendingApprovals.Where(s => s.ReferenceId == request.ReferenceId);
        }

        return await pendingApprovals.ProjectTo<RequestDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, string.IsNullOrEmpty(request.SortBy) ? "UpdateDate" : request.SortBy);
    }
}