using AutoMapper;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using MediatR;

namespace LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconcilationDetail;

public class PerformReconciliationDetailCommand : IRequest<ReconciliationDetailsResponseDto>
{
    public DateTime ReconciliationDate { get; set; }
    public Guid VendorId { get; set; }
    public Guid InstitutionId { get; set; }
}

public class GetReconciliationDetailQueryHandler : IRequestHandler<PerformReconciliationDetailCommand, ReconciliationDetailsResponseDto>
{
    private readonly IBillingService _billingService;
    private readonly IMapper _mapper;

    public GetReconciliationDetailQueryHandler(IMapper mapper, IBillingService billingService)
    {
        _mapper = mapper;
        _billingService = billingService;
    }

    public async Task<ReconciliationDetailsResponseDto> Handle(PerformReconciliationDetailCommand request, CancellationToken cancellationToken)
    {
        var reconciliationDetailsRequest = _mapper.Map<ReconciliationDetailsRequest>(request);
        var reconciliationDetails = await _billingService.GetReconciliationDetailsAsync(reconciliationDetailsRequest);

        return _mapper.Map<ReconciliationDetailsResponseDto>(reconciliationDetails);
    }
}