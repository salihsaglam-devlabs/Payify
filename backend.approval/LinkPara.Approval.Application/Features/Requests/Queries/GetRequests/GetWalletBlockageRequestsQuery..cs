using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Approval.Domain.Enums;
using LinkPara.Approval.Models.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Approval.Application.Features.Requests.Queries.GetRequests;

public class GetWalletBlockageRequestsQuery : SearchQueryParams, IRequest<PaginatedList<RequestWalletBlockageDto>>
{
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public ActionType? ActionType { get; set; }
    public RequestOperationType? OperationType { get; set; }
    public List<ApprovalStatus>? Statuses { get; set; }
}

public class GetWalletBlockageRequestsQueryHandler : IRequestHandler<GetWalletBlockageRequestsQuery, PaginatedList<RequestWalletBlockageDto>>
{
    private const string WalletBlockagesResourceIdentifier = "WalletBlockages";
    private readonly IGenericRepository<Request> _requestRepository;
    private readonly IMapper _mapper;

    public GetWalletBlockageRequestsQueryHandler(IGenericRepository<Request> requestRepository,
        IMapper mapper)
    {
        _requestRepository = requestRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<RequestWalletBlockageDto>> Handle(GetWalletBlockageRequestsQuery request, CancellationToken cancellationToken)
    {
        var walletBlockageRequests = _requestRepository.GetAll()
            .Where(x => x.Resource.Contains(WalletBlockagesResourceIdentifier));

        if (request.Statuses is not null && request.Statuses.Any())
        {
            walletBlockageRequests = walletBlockageRequests.Where(s => request.Statuses.Contains(s.Status));
        }
        if (request.OperationType is not null)
        {
            walletBlockageRequests = walletBlockageRequests.Where(s => s.OperationType == request.OperationType);
        }
        if (request.ActionType is not null)
        {
            walletBlockageRequests = walletBlockageRequests.Where(s => s.ActionType == request.ActionType);
        }

        if (request.DateStart is not null)
        {
            walletBlockageRequests = walletBlockageRequests.Where(s => s.CreateDate >= request.DateStart);
        }

        if (request.DateEnd is not null)
        {
            walletBlockageRequests = walletBlockageRequests.Where(s => s.CreateDate <= request.DateEnd);
        }

        return await walletBlockageRequests
            .ProjectTo<RequestWalletBlockageDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, string.IsNullOrEmpty(request.SortBy) ? "CreateDate" : request.SortBy);
    }
}
