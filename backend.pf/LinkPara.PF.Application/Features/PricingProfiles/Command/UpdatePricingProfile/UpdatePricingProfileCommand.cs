using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using MediatR;

namespace LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePricingProfile;

public class UpdatePricingProfileCommand : IRequest, IMapFrom<PricingProfile>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime ActivationDate { get; set; }
    public decimal PerTransactionFee { get; set; }
    public List<PricingProfileItemDto> PricingProfileItems { get; set; }
}

public class UpdatePricingProfileCommandHandler : IRequestHandler<UpdatePricingProfileCommand>
{
    private readonly IPricingProfileService _pricingProfileService;

    public UpdatePricingProfileCommandHandler(IPricingProfileService pricingProfileService)
    {
        _pricingProfileService = pricingProfileService;
    }
    public async Task<Unit> Handle(UpdatePricingProfileCommand request, CancellationToken cancellationToken)
    {
        await _pricingProfileService.UpdateAsync(request);

        return Unit.Value;
    }
}
