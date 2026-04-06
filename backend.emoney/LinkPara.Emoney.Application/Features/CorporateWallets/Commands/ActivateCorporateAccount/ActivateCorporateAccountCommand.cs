using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateCorporateAccount;

public class ActivateCorporateAccountCommand : IRequest
{
    public Guid Id { get; set; }
}

public class ActivateCorporateAccountCommandHandler : IRequestHandler<ActivateCorporateAccountCommand>
{
    private readonly ICorporateWalletService _corporateWalletService;

    public ActivateCorporateAccountCommandHandler(ICorporateWalletService corporateWalletService)
    {
        _corporateWalletService = corporateWalletService;
    }

    public async Task<Unit> Handle(ActivateCorporateAccountCommand request, CancellationToken cancellationToken)
    {
        await _corporateWalletService.ActivateCorporateAccountAsync(request);
        return Unit.Value;
    }
}