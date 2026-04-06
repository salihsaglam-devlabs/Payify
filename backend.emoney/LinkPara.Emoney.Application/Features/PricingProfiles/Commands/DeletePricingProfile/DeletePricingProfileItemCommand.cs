using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Commands.DeletePricingProfile;

public class DeletePricingProfileItemCommand : IRequest
{
    public Guid Id { get; set; } 
}

public class DeletePricingProfileCommandHandler : IRequestHandler<DeletePricingProfileItemCommand>
{
    private readonly IPricingProfileService _service;

    public DeletePricingProfileCommandHandler(IPricingProfileService service)
    {
        _service = service;
    }

    public async Task<Unit> Handle(DeletePricingProfileItemCommand request, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(request);

        return Unit.Value;
    }
}