using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using MediatR;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfileItem;

public class UpdatePricingProfileItemCommand : IRequest
{
    public Guid Id { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal Fee { get; set; }
    public decimal CommissionRate { get; set; }
    public WalletType WalletType { get; set; }
}

public class UpdatePricingProfileItemCommandHandler : IRequestHandler<UpdatePricingProfileItemCommand>
{
    private readonly IPricingProfileService _service;

    public UpdatePricingProfileItemCommandHandler(IPricingProfileService service)
    {
        _service = service;
    }

    public async Task<Unit> Handle(UpdatePricingProfileItemCommand request, CancellationToken cancellationToken)
    {
        await _service.UpdateProfileItemAsync(request);

        return Unit.Value;
    }
}
