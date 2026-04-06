using AutoMapper;
using LinkPara.Billing.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationInstitutionDetail;

public class PerformReconciliationInstitutionDetailCommand : IRequest<InstitutionPaymentDetailResponseDto>
{
    public Guid InstitutionSummaryId { get; set; }
}

public class GetInstitutionPaymentsDetailQueryHandler : IRequestHandler<PerformReconciliationInstitutionDetailCommand, InstitutionPaymentDetailResponseDto>
{
    private readonly IBillingService _billingService;
    private readonly IMapper _mapper;

    public GetInstitutionPaymentsDetailQueryHandler(IBillingService billingService, IMapper mapper)
    {
        _billingService = billingService;
        _mapper = mapper;
    }

    public async Task<InstitutionPaymentDetailResponseDto> Handle(PerformReconciliationInstitutionDetailCommand request, CancellationToken cancellationToken)
    {
        var paymentDetailsResponse = await _billingService.GetInstitutionPaymentDetailsAsync(request.InstitutionSummaryId);

        return _mapper.Map<InstitutionPaymentDetailResponseDto>(paymentDetailsResponse);
    }
}