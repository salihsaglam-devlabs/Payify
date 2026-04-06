using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.AddUser;

public class AddCorporateWalletUserCommand : IRequest
{
    public Guid AccountId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public List<Guid> Roles { get; set; }
}

public class AddCorporateWalletUserCommandHandler : IRequestHandler<AddCorporateWalletUserCommand>
{
    private readonly ICorporateWalletService _corporateWalletService;

    public AddCorporateWalletUserCommandHandler(
        ICorporateWalletService corporateWalletService)
    {
        _corporateWalletService = corporateWalletService;
    }

    public async Task<Unit> Handle(AddCorporateWalletUserCommand request, CancellationToken cancellationToken)
    {
        await _corporateWalletService.AddCorporateWalletUserAsync(request);
        return Unit.Value;
    }
}