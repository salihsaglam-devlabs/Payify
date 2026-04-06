using AutoMapper;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using MediatR;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconcilationSummary;

public class PerformReconcilationSummaryCommand : IRequest<ReconciliationSummaryResponseDto>
{
    public Guid VendorId { get; set; }
    public DateTime ReconciliationDate { get; set; }
}

public class GetReconcilationSummaryQueryHandler : IRequestHandler<PerformReconcilationSummaryCommand, ReconciliationSummaryResponseDto>
{
    private readonly IBillingService _billingService;
    private readonly IMapper _mapper;

    public GetReconcilationSummaryQueryHandler(IBillingService billingService, IMapper mapper)
    {
        _billingService = billingService;
        _mapper = mapper;
    }

    public async Task<ReconciliationSummaryResponseDto> Handle(PerformReconcilationSummaryCommand request, CancellationToken cancellationToken)
    {
        var summaryRequest = _mapper.Map<ReconciliationSummaryRequest>(request);
        var summaryResponse =  await _billingService.GetReconciliationSummaryAsync(summaryRequest);

        return _mapper.Map<ReconciliationSummaryResponseDto>(summaryResponse);
    }
}