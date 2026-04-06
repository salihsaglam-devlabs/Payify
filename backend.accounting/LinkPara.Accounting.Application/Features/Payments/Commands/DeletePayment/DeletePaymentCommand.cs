
using LinkPara.Accounting.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Accounting.Application.Features.Payments.Commands.DeletePayment;

public class DeletePaymentCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand>
{
    private readonly IPaymentService _paymentService;

    public DeletePaymentCommandHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Unit> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        return await _paymentService.DeletePaymentAsync(request);
    }
}
