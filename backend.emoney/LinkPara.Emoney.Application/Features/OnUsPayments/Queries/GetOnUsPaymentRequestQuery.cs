using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OnUsPayments.Queries;

public class GetOnUsPaymentRequestQuery : IRequest<OnUsPaymentRequest>
{
    public Guid OnUsPaymentRequestId { get; set; }
}

public class GetOnUsPaymentRequestQueryHandler : IRequestHandler<GetOnUsPaymentRequestQuery, OnUsPaymentRequest>
{
    private readonly IOnUsPaymentService _onUsPaymentService;

    public GetOnUsPaymentRequestQueryHandler(IOnUsPaymentService onUsPaymentService)
    {
        _onUsPaymentService = onUsPaymentService;
    }

    public async Task<OnUsPaymentRequest> Handle(GetOnUsPaymentRequestQuery request, CancellationToken cancellationToken)
    {
        return await _onUsPaymentService.GetOnUsPaymentDetailsAsync(request.OnUsPaymentRequestId);
    }
}
