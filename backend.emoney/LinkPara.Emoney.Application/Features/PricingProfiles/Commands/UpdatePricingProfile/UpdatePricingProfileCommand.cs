using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using MediatR;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfile;

public class UpdatePricingProfileCommand : IRequest
{
    public Guid Id { get; set; }
    public DateTime ActivationDateStart { get; set; }
    public int? BankCode { get; set; }
    public string CurrencyCode { get; set; }
    public TransferType TransferType { get; set; }
    public List<PricingProfileItemUpdateModel> ProfileItems { get; set; }
}

public class UpdatePricingProfileCommandHandler : IRequestHandler<UpdatePricingProfileCommand>
{
    private readonly IPricingProfileService _service;

    public UpdatePricingProfileCommandHandler(IPricingProfileService service)
    {
        _service = service;
    }

    public async Task<Unit> Handle(UpdatePricingProfileCommand request, CancellationToken cancellationToken)
    {
        await _service.UpdateAsync(request);

        return Unit.Value;
    }
}
