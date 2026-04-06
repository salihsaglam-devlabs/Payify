using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetConsentDetail;
public class GetConsentDetailQuery : IRequest<GetConsentDetailResponse>
{
    public string ConsentId { get; set; }
}

public class GetConsentDetailQueryHandler : IRequestHandler<GetConsentDetailQuery, GetConsentDetailResponse>
{
    private readonly IConsentOperationsService _consentOperationsService;

    public GetConsentDetailQueryHandler(
         IConsentOperationsService consentOperationsService)
    {
        _consentOperationsService = consentOperationsService;
    }

    public async Task<GetConsentDetailResponse> Handle(GetConsentDetailQuery request,
        CancellationToken cancellationToken)
    {

        return await _consentOperationsService.GetConsentDetailAsync(request);
                

    }
}
