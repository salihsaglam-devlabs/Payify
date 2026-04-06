using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateUser;

public class ActivateCorporateWalletUserCommand : IRequest
{

    public Guid AccountId { get; set; }
    public Guid AccountUserId { get; set; }
}

public class ActivateCorporateWalletUserCommandHandler : IRequestHandler<ActivateCorporateWalletUserCommand>
{
    private readonly ICorporateWalletService _corporateWalletService;

    public ActivateCorporateWalletUserCommandHandler(ICorporateWalletService corporateWalletService)
    {
        _corporateWalletService = corporateWalletService;
    }

    public async Task<Unit> Handle(ActivateCorporateWalletUserCommand request, CancellationToken cancellationToken)
    {
        await _corporateWalletService.ActivateCorporateWalletUserAsync(request);
        return Unit.Value;
    }
}
