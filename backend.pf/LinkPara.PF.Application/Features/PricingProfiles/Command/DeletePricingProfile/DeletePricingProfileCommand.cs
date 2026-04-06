using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.PricingProfiles.Command.DeletePricingProfile;

public class DeletePricingProfileCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeletePricingProfileCommandHandler : IRequestHandler<DeletePricingProfileCommand>
{
    private readonly IPricingProfileService _pricingProfileService;

    public DeletePricingProfileCommandHandler(IPricingProfileService pricingProfileService)
    {
        _pricingProfileService = pricingProfileService;
    }

    public async Task<Unit> Handle(DeletePricingProfileCommand request, CancellationToken cancellationToken)
    {
        await _pricingProfileService.DeleteAsync(request);

        return Unit.Value;
    }
}
