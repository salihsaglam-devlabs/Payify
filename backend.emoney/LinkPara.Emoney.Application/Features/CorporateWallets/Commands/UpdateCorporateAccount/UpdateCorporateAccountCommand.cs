using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateCorporateAccount;

public class UpdateCorporateAccountCommand : IRequest
{
    public Guid Id {  get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string LandPhone { get; set; }
    public string WebSiteUrl { get; set; }
    public string Name { get; set; }
    public string PostalCode { get; set; }
    public string Address { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
    public int District { get; set; }
    public string DistrictName { get; set; }
}
public class UpdateCorporateAccountCommandHandler : IRequestHandler<UpdateCorporateAccountCommand>
{
    private readonly ICorporateWalletService _corporateWalletService;

    public UpdateCorporateAccountCommandHandler(ICorporateWalletService corporateWalletService)
    {
        _corporateWalletService = corporateWalletService;
    }

    public async Task<Unit> Handle(UpdateCorporateAccountCommand request, CancellationToken cancellationToken)
    {
        await _corporateWalletService.UpdateCorporateAccountAsync(request);
        return Unit.Value;
    }
}
