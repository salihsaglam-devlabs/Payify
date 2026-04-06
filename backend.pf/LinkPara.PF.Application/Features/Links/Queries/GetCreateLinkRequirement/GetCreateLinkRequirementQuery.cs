
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetMccByCode;
using LinkPara.PF.Application.Features.MerchantCategoryCodes;
using MediatR;

namespace LinkPara.PF.Application.Features.Links.Queries.GetCreateLinkRequirement;

public class GetCreateLinkRequirementQuery : IRequest<LinkRequirementResponse>
{
    public Guid MerchantId { get; set; }
}

public class GetCreateLinkRequirementQueryHandler : IRequestHandler<GetCreateLinkRequirementQuery, LinkRequirementResponse>
{
    private readonly ILinkService _linkService;

    public GetCreateLinkRequirementQueryHandler(ILinkService linkService)
    {
        _linkService = linkService;
    }
    public async Task<LinkRequirementResponse> Handle(GetCreateLinkRequirementQuery request, CancellationToken cancellationToken)
    {
        return await _linkService.GetCreateLinkRequirements(request);
    }
}
