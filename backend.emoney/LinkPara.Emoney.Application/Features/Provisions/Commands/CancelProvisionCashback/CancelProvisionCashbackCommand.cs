

using LinkPara.Emoney.Application.Commons.Attributes;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Provisions.Commands.CancelProvisionCashback;

public class CancelProvisionCashbackCommand : IRequest<ProvisionCashbackResponse>
{
    [Audit]
    public string ProvisionReference { get; set; }
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public Guid UserId { get; set; }

}

public class CancelProvisionCashbackCommandHandler : IRequestHandler<CancelProvisionCashbackCommand, ProvisionCashbackResponse>
{
    private readonly IProvisionService _provisionService;

    public CancelProvisionCashbackCommandHandler(IProvisionService provisionService)
    {
        _provisionService = provisionService;
    }

    public async Task<ProvisionCashbackResponse> Handle(CancelProvisionCashbackCommand request, CancellationToken cancellationToken)
    {
        return await _provisionService.CancelProvisionCashbackAsync(request, cancellationToken);
    }
}
