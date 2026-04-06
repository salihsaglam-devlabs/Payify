
using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.IWalletOtpCodes.Commands;

public class SendIWalletOtpCodeCommand : IRequest<SendIWalletOtpCodeResponseDto>
{
    public string WalletNumber { get; set; }
    public string Amount { get; set; }
    public string MerchantName { get; set; }
    public string OtpPassword { get; set; }
    public int Type { get; set; }
}

public class SendIWalletOtpCodeCommandHandler : IRequestHandler<SendIWalletOtpCodeCommand, SendIWalletOtpCodeResponseDto>
{
    private readonly IIWalletOtpService _service;

    public SendIWalletOtpCodeCommandHandler(IIWalletOtpService service)
    {
        _service = service;
    }

    public async Task<SendIWalletOtpCodeResponseDto> Handle(SendIWalletOtpCodeCommand request, CancellationToken cancellationToken)
    {
        return await _service.NotifyOtpCodeAsync(request, cancellationToken);
    }
}