using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetWaitingApprovalConsents;
public class GetWaitingApprovalConsentQuery : IRequest<GetWaitingApprovalConsentResponse>
{
    public string AccountId { get; set; }
}

public class GetWaitingApprovalConsentQueryHandler : IRequestHandler<GetWaitingApprovalConsentQuery, GetWaitingApprovalConsentResponse>
{
    private readonly IConsentOperationsService _consentOperationsService;

    public GetWaitingApprovalConsentQueryHandler(
          IConsentOperationsService consentOperationsService)
    {
        _consentOperationsService = consentOperationsService;
    }

    public async Task<GetWaitingApprovalConsentResponse> Handle(GetWaitingApprovalConsentQuery request,
        CancellationToken cancellationToken)
    {

        return await _consentOperationsService.GetWaitingApprovalConsentsAsync(request);
                

    }
}
