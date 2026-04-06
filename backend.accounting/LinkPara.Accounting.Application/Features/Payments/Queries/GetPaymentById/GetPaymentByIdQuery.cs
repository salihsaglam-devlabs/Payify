using LinkPara.Accounting.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Accounting.Application.Features.Payments.Queries.GetPaymentById;

public class GetPaymentByIdQuery : IRequest<PaymentDto>
{
    public Guid Id { get; set; }
}

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
{
    private readonly IPaymentService _paymentService;

    public GetPaymentByIdQueryHandler(
        IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _paymentService.GetByIdAsync(request.Id);

    }
}
