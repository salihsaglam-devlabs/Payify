using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OnUsPayments.Queries;

public class GetOnUsPaymentQuery : SearchQueryParams, IRequest<PaginatedList<OnUsPaymentRequest>>
{
    public Guid? TransactionId { get; set; }
    public string OrderId { get; set; }
    public string WalletNumber { get; set; }
    public string MerchantName { get; set; }
    public DateTime? TransactionDate { get; set; }
    public OnUsPaymentStatus Status { get; set; }
    public string UserName { get; set; }
}

public class GetOnUsPaymentQueryHandler : IRequestHandler<GetOnUsPaymentQuery, PaginatedList<OnUsPaymentRequest>>
{
    private readonly IOnUsPaymentService _onUsPaymentService;

    public GetOnUsPaymentQueryHandler(IOnUsPaymentService onUsPaymentService)
    {
        _onUsPaymentService = onUsPaymentService;
    }

    public async Task<PaginatedList<OnUsPaymentRequest>> Handle(GetOnUsPaymentQuery request, CancellationToken cancellationToken)
    {
        return await _onUsPaymentService.GetOnUsPaymentsAsync(request);
    }
}
