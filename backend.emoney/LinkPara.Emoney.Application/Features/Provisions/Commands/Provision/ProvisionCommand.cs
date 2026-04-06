using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using LinkPara.HttpProviders.Emoney.Enums;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Provisions.Commands.Provision;

public class ProvisionCommand : IRequest<ProvisionResponse>
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public ProvisionSource ProvisionSource { get; set; }
    public string Description { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public Guid? PartnerId { get; set; }
    public string Tag { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal BsmvAmount { get; set; }
    public string OrderId { get; set; }
}

public class ProvisionCommandHandler : IRequestHandler<ProvisionCommand, ProvisionResponse>
{
    private readonly IProvisionService _provisionService;
    
    public ProvisionCommandHandler(IProvisionService provisionService)
    {
        _provisionService = provisionService;
    }
    
    public async Task<ProvisionResponse> Handle(ProvisionCommand request, CancellationToken cancellationToken)
    {
        return await _provisionService.ProvisionAsync(request, cancellationToken);
    }
}