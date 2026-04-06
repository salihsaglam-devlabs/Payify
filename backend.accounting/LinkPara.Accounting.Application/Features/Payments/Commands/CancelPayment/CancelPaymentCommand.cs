using LinkPara.Accounting.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Accounting.Application.Features.Payments.Commands.CancelPayment;

public class CancelPaymentCommand : IRequest
{
    public Guid ClientReferenceId { get; set; }
}

public class CancelPaymentCommandHandler : IRequestHandler<CancelPaymentCommand>
{
    private readonly IPaymentService _paymentService;

    public CancelPaymentCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Unit> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        await _paymentService.CancelPaymentAsync(request);

        return Unit.Value;
    }
}