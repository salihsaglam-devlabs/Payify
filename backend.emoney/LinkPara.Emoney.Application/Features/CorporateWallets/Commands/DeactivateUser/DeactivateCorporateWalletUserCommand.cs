using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeleteUser;

public class DeactivateCorporateWalletUserCommand : IRequest
{
    public Guid AccountId { get; set; }
    public Guid AccountUserId { get; set; }
}

public class DeactivateCorporateWalletUserCommandHandler : IRequestHandler<DeactivateCorporateWalletUserCommand>
{
    private readonly ICorporateWalletService _corporateWalletService;

    public DeactivateCorporateWalletUserCommandHandler(ICorporateWalletService corporateWalletService)
    {
        _corporateWalletService = corporateWalletService;
    }

    public async Task<Unit> Handle(DeactivateCorporateWalletUserCommand request, CancellationToken cancellationToken)
    {
        await _corporateWalletService.DeactivateCorporateWalletUserAsync(request);
        return Unit.Value;
    }
}
