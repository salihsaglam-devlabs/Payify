using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetActiveConsentList;
public class GetActiveConsentListQuery : IRequest<List<ConsentDto>>
{
    public string AccountId { get; set; }
    public ConsentType ConsentType { get; set; }
}

public class GetActiveConsentListQueryHandler : IRequestHandler<GetActiveConsentListQuery, List<ConsentDto>>
{
    private readonly IConsentOperationsService _consentOperationsService;

    public GetActiveConsentListQueryHandler(
        IConsentOperationsService consentOperationsService)
    {
        _consentOperationsService = consentOperationsService;
    }

    public async Task<List<ConsentDto>> Handle(GetActiveConsentListQuery request,
        CancellationToken cancellationToken)
    {
        return await _consentOperationsService.GetActiveConsentListAsync(request);
    }
}
