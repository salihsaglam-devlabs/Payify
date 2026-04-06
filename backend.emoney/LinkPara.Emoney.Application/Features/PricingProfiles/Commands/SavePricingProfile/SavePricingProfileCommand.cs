using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Commands.SavePricingProfile;

public class SavePricingProfileCommand : IRequest
{
    public DateTime ActivationDateStart { get; set; }
    public int? BankCode { get; set; }
    public string CurrencyCode { get; set; }
    public TransferType TransferType { get; set; }
    public List<PricingProfileItemModel> ProfileItems { get; set; }
    public CardType CardType { get; set; }
}

public class SavePricingProfileCommandHandler : IRequestHandler<SavePricingProfileCommand>
{
    private readonly IPricingProfileService _service;

    public SavePricingProfileCommandHandler(IPricingProfileService service)
    {
        _service = service;
    }

    public async Task<Unit> Handle(SavePricingProfileCommand request, CancellationToken cancellationToken)
    {
        await _service.SaveAsync(request);

        return Unit.Value;
    }
}
