using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Queries.GetPaymentDetails;

public class GetPaymentDetailQuery : IRequest<PosPaymentDetailResponse>
{
    public string OrderId { get; set; }
}

public class GetPaymentDetailQueryHandler : IRequestHandler<GetPaymentDetailQuery, PosPaymentDetailResponse>
{
    private readonly IPaymentDetailService _paymentService;
    public GetPaymentDetailQueryHandler(IPaymentDetailService paymentService)
    {
        _paymentService = paymentService;
    }
    public async Task<PosPaymentDetailResponse> Handle(GetPaymentDetailQuery request, CancellationToken cancellationToken)
    {
        return await _paymentService.GetPaymentDetailAsync(request.OrderId);
    }
}
