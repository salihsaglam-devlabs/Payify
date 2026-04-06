using LinkPara.Emoney.Application.Commons.Attributes;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Provisions.Commands.ProvisionCashback;

public class ProvisionCashbackCommand : IRequest<ProvisionCashbackResponse>
{
    [Audit]
    public string ProvisionReference { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public Guid UserId { get; set; }
}

public class ProvisionCashbackCommandHandler : IRequestHandler<ProvisionCashbackCommand, ProvisionCashbackResponse>
{
    private readonly IProvisionService _provisionService;

    public ProvisionCashbackCommandHandler(IProvisionService provisionService)
    {
        _provisionService = provisionService;
    }

    public async Task<ProvisionCashbackResponse> Handle(ProvisionCashbackCommand request, CancellationToken cancellationToken)
    {
        return await _provisionService.ProvisionCashbackAsync(request, cancellationToken);
    }
}
