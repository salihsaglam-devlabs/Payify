
using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.Agreements.Queries.GetAgreements;

public class GetAgreementsQuery : IRequest<List<AgreementDto>>
{
}

public class GetAgreementsQueryHandler : IRequestHandler<GetAgreementsQuery, List<AgreementDto>>
{
    private readonly IIWalletAgreementService _iwalletAgreementService;

    public GetAgreementsQueryHandler(IIWalletAgreementService iwalletAgreementService)
    {
        _iwalletAgreementService = iwalletAgreementService;
    }

    public async Task<List<AgreementDto>> Handle(GetAgreementsQuery request, CancellationToken cancellationToken)
    {
        return await _iwalletAgreementService.GetAgreementsAsync();
    }
}
