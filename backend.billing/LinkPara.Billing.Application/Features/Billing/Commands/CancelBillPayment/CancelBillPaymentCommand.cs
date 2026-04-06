using AutoMapper;
using LinkPara.Billing.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Billing.Application.Features.Billing.Commands.CancelBillPayment;

public class CancelBillPaymentCommand : IRequest<BillCancelResponseDto>
{
    public Guid TransactionId { get; set; }
    public string CancellationReason { get; set; }
}

public class CancelBillPaymentCommandHandler : IRequestHandler<CancelBillPaymentCommand, BillCancelResponseDto>
{
    private readonly IMapper _mapper;
    private readonly IBillingService _billingService;

    public CancelBillPaymentCommandHandler(IMapper mapper, IBillingService billingService)
    {
        _mapper = mapper;
        _billingService = billingService;
    }

    public async Task<BillCancelResponseDto> Handle(CancelBillPaymentCommand request, CancellationToken cancellationToken)
    {
        var billPaymentCancelResponse = await _billingService.CancelBillPaymentAsync(request.TransactionId, request.CancellationReason);

        return _mapper.Map<BillCancelResponseDto>(billPaymentCancelResponse);
    }
}