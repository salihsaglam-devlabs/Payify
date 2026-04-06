using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletQrCodes.Commands.CreateQrCode;

public class CreateQrCodeCommand : IRequest<IWalletQrCodeDto>
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
}

public class CreateQrCodeQueryHandler : IRequestHandler<CreateQrCodeCommand, IWalletQrCodeDto>
{
    private readonly IIWalletQrCodeService _qrCodeService;

    public CreateQrCodeQueryHandler(IIWalletQrCodeService qrCodeService)
    {
        _qrCodeService = qrCodeService;
    }

    public async Task<IWalletQrCodeDto> Handle(CreateQrCodeCommand request, CancellationToken cancellationToken)
    {
        return await _qrCodeService.GenerateQrCodeAsync(request.UserId, request.WalletNumber);
    }
}
