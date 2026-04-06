using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Features.IWalletQrCodes;
using LinkPara.CampaignManagement.Domain.Enums;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCards.Commands;

public class CreateCardCommand : IRequest<IWalletQrCodeDto>
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public string FullName { get; set; }
    public string IdentityNumber { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string AddressDetail { get; set; }
    public int CityId { get; set; }
    public int TownId { get; set; }
    public UserType UserType { get; set; }
    public bool IsApprovedIndividualFrameworkAgreement { get; set; }
    public string IndividualFrameworkAgreementVersion { get; set; }
    public bool IsApprovedPreliminaryInformationAgreement { get; set; }
    public string PreliminaryInformationAgreementVersion { get; set; }
    public bool IsApprovedKvkkAgreement { get; set; }
    public string KvkkAgreementVersion { get; set; }
    public bool IsApprovedCommercialElectronicCommunicationAggrement { get; set; }
    public string CommercialElectronicCommunicationAggrementVersion { get; set; }
}

public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, IWalletQrCodeDto>
{
    private readonly IIWalletCardService _cardService;
    private readonly IIWalletQrCodeService _qrCodeService;

    public CreateCardCommandHandler(IIWalletCardService cardService, IIWalletQrCodeService qrCodeService)
    {
        _cardService = cardService;
        _qrCodeService = qrCodeService;
    }

    public async Task<IWalletQrCodeDto> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        var card = await _cardService.CreateCardAsync(request);
        return await _qrCodeService.GenerateQrCodeAsync(card.UserId, card.WalletNumber);
    }
}
