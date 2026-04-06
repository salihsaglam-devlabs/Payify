using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.HostedPayments.Queries.GetHppTransactions;

public class GetHppTransactionsQuery : SearchQueryParams, IRequest<PaginatedList<HostedPaymentDto>>
{
    public Guid MerchantId { get; set; }
    public string TrackingId { get; set; }
    public string OrderId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public ChannelStatus? HppStatus { get; set; }
    public ChannelPaymentStatus? HppPaymentStatus { get; set; }
    public WebhookStatus? WebhookStatus { get; set; }
    public HppPageViewType? PageViewType { get; set; }
    public DateTime? ExpiryDateStart { get; set; }
    public DateTime? ExpiryDateEnd { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}

public class GetHppTransactionsQueryHandler : IRequestHandler<GetHppTransactionsQuery, PaginatedList<HostedPaymentDto>>
{
    private readonly IHostedPaymentService _hostedPaymentService;

    public GetHppTransactionsQueryHandler(IHostedPaymentService hostedPaymentService)
    {
        _hostedPaymentService = hostedPaymentService;
    }
    public async Task<PaginatedList<HostedPaymentDto>> Handle(GetHppTransactionsQuery request, CancellationToken cancellationToken)
    {
        return await _hostedPaymentService.GetFilterListAsync(request);
    }
}