
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Provisions.Queries.InquireProvision;

public class InquireProvisionQuery : IRequest<InquireProvisionResponse>
{
    public string ConversationId { get; set; }
    public string ProvisionReference { get; set; }
}

public class GetPartnerProvisionStatusQueryHandler : IRequestHandler<InquireProvisionQuery, InquireProvisionResponse>
{
    private readonly IProvisionService _provisionService;

    public GetPartnerProvisionStatusQueryHandler(IProvisionService provisionService)
    {
        _provisionService = provisionService;
    }

    public async Task<InquireProvisionResponse> Handle(InquireProvisionQuery request, CancellationToken cancellationToken)
    {
        return await _provisionService.InquireProvisionAsync(request);
    }
}
