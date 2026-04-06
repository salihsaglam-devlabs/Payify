using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateUser;

public class UpdateCorporateWalletUserCommand : IRequest
{
    public Guid AccountUserId { get; set; }
    public Guid AccountId { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public List<Guid> Roles { get; set; }
}

public class UpdateCorporateWalletUserCommandHandler : IRequestHandler<UpdateCorporateWalletUserCommand>
{
    private readonly ICorporateWalletService _corporateWalletService;

    public UpdateCorporateWalletUserCommandHandler(ICorporateWalletService corporateWalletService)
    {
        _corporateWalletService = corporateWalletService;
    }

    public async Task<Unit> Handle(UpdateCorporateWalletUserCommand request, CancellationToken cancellationToken)
    {
        await _corporateWalletService.UpdateCorporateWalletUserAsync(request);
        return Unit.Value;
    }
}
