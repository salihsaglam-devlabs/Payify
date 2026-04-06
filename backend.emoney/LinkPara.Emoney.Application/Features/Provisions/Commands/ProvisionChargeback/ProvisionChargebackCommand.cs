using LinkPara.Emoney.Application.Commons.Attributes;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using LinkPara.HttpProviders.Emoney.Enums;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Provisions.Commands.ProvisionCashback;

public class ProvisionChargebackCommand : IRequest<ProvisionChargebackResponse>
{
    [Audit]
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public ProvisionSource ProvisionSource { get; set; }
    public string Description { get; set; }
    [Audit]
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string ProvisionReference{ get; set; }
}

public class ProvisionChargebackCommandHandler : IRequestHandler<ProvisionChargebackCommand, ProvisionChargebackResponse>
{
    private readonly IProvisionService _provisionService;

    public ProvisionChargebackCommandHandler(IProvisionService provisionService)
    {
        _provisionService = provisionService;
    }

    public async Task<ProvisionChargebackResponse> Handle(ProvisionChargebackCommand request, CancellationToken cancellationToken)
    {
        return await _provisionService.ProvisionChargebackAsync(request, cancellationToken);
    }
}
