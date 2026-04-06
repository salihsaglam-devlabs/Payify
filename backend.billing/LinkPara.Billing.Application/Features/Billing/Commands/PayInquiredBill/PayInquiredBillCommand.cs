using AutoMapper;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Domain.Enums;
using MediatR;

namespace LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;

public class PayInquiredBillCommand : IRequest<BillPaymentResponseDto>
{
    public string RequestId { get; set; }
    public Guid InstitutionId { get; set; }
    public Bill Bill { get; set; }
    public string WalletNumber { get; set; }
    public string PayeeFullName { get; set; }
    public string PayeeMobile { get; set; }
    public string PayeeEmail { get; set; }
    public PaymentSource PaymentSource { get; set; }
}

public class PayInquiredBillCommandHandler : IRequestHandler<PayInquiredBillCommand, BillPaymentResponseDto>
{
    private readonly IBillingService _billingService;
    private readonly IMapper _mapper;


    public PayInquiredBillCommandHandler(IBillingService billingService, IMapper mapper)
    {
        _billingService = billingService;
        _mapper = mapper;
    }

    public async Task<BillPaymentResponseDto> Handle(PayInquiredBillCommand request, CancellationToken cancellationToken)
    {
        if (request.Bill.Date == DateTime.MinValue)
        {
            request.Bill.Date = null;
        }

        var paymentResponse = await _billingService.PayInquiredBillAsync(request);

        return _mapper.Map<BillPaymentResponseDto>(paymentResponse);
    }
}