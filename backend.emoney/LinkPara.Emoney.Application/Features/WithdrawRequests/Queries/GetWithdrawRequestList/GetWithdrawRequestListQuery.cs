using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.WithdrawRequests.Queries.GetWithdrawRequestList;

public class GetWithdrawRequestListQuery : SearchQueryParams, IRequest<PaginatedList<WithdrawRequestAdminDto>>
{
    public WithdrawStatus? WithdrawStatus { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public string CurrencyCode { get; set; }
    public string Description { get; set; }
    public string WalletNumber { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? UserId { get; set; }
    public TransferType? TransferType { get; set; }
}

public class GetWithdrawRequestListQueryHandler : IRequestHandler<GetWithdrawRequestListQuery,
    PaginatedList<WithdrawRequestAdminDto>>
{
    private readonly IWithdrawRequestService _service;

    public GetWithdrawRequestListQueryHandler(IWithdrawRequestService service)
    {
        _service = service;
    }

    public async Task<PaginatedList<WithdrawRequestAdminDto>> Handle(GetWithdrawRequestListQuery request,
        CancellationToken cancellationToken)
    {
        return await _service.GetWithdrawRequestListAsync(request);
    }
}