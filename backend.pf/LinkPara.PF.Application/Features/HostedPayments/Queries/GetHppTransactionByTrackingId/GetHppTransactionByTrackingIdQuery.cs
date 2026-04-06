using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.HostedPayments.Queries.GetHppTransactionByTrackingId;

public class GetHppTransactionByTrackingIdQuery : IRequest<HppTransactionResponse>
{
    public string TrackingId { get; set; }
    public Guid MerchantId { get; set; }
}

public class GetHppTransactionByTrackingIdQueryHandler : IRequestHandler<GetHppTransactionByTrackingIdQuery, HppTransactionResponse>
{
    private readonly IHostedPaymentService _hostedPaymentService;
    
    public GetHppTransactionByTrackingIdQueryHandler(IHostedPaymentService hostedPaymentService)
    {
        _hostedPaymentService = hostedPaymentService;
    }

    public async Task<HppTransactionResponse> Handle(GetHppTransactionByTrackingIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _hostedPaymentService.GetHppTransactionAsync(request.TrackingId, request.MerchantId);
    }
}