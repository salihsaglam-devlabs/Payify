using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeactivateCorporateAccount;

public class DeactivateCorporateAccountCommand : IRequest
{
    public Guid Id {  get; set; }
}

public class DeactivateCorporateAccountCommandHandler : IRequestHandler<DeactivateCorporateAccountCommand>
{
    private readonly ICorporateWalletService _corporateWalletService;

    public DeactivateCorporateAccountCommandHandler(ICorporateWalletService corporateWalletService)
    {
        _corporateWalletService = corporateWalletService;
    }

    public async Task<Unit> Handle(DeactivateCorporateAccountCommand request, CancellationToken cancellationToken)
    {
        await _corporateWalletService.DeactivateCorporateAccountAsync(request);
        return Unit.Value;
    }
}