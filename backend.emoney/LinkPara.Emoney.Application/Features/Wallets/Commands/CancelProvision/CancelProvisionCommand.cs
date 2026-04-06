using LinkPara.Emoney.Application.Commons.Attributes;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.CancelProvision;

public class CancelProvisionCommand : IRequest<ProvisionResponse>
{
    [Audit]
    public string ConversationId { get; set; }
}

public class CancelProvisionCommandHandler : IRequestHandler<CancelProvisionCommand, ProvisionResponse>
{
    private readonly IProvisionService _provisionService;

    public CancelProvisionCommandHandler(IProvisionService provisionService)
    {
        _provisionService = provisionService;
    }

    public async Task<ProvisionResponse> Handle(CancelProvisionCommand request, CancellationToken cancellationToken)
    {
        return await _provisionService.CancelProvisionAsync(request, cancellationToken);
    }
}