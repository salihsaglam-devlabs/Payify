using LinkPara.Emoney.Application.Commons.Attributes;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using LinkPara.HttpProviders.Emoney.Enums;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Provisions.Commands.ReturnProvision;

public class ReturnProvisionCommand : IRequest<ProvisionResponse>
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public ProvisionSource ProvisionSource { get; set; }
    public string Description { get; set; }
    [Audit]
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string ProvisionReferenceNumber { get; set; }
}

public class ReturnProvisionCommandHandler : IRequestHandler<ReturnProvisionCommand, ProvisionResponse>
{
    private readonly IProvisionService _provisionService;

    public ReturnProvisionCommandHandler(IProvisionService provisionService)
    {
        _provisionService = provisionService;
    }

    public async Task<ProvisionResponse> Handle(ReturnProvisionCommand request, CancellationToken cancellationToken)
    {
        return await _provisionService.ReturnProvisionAsync(request,cancellationToken);
    }
}