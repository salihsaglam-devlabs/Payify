using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Features.IWalletOtpCodes;
using LinkPara.CampaignManagement.Application.Features.IWalletOtpCodes.Commands;
using LinkPara.SharedModels.BusModels.Commands.Notification;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class IWalletOtpService : IIWalletOtpService
{
    private readonly IIWalletCardService _iWalletCardService;
    private readonly ISmsSenderService _smsSenderService;

    public IWalletOtpService(IIWalletCardService iWalletCardService,
        ISmsSenderService smsSenderService)
    {
        _iWalletCardService = iWalletCardService;
        _smsSenderService = smsSenderService;
    }

    public async Task<SendIWalletOtpCodeResponseDto> NotifyOtpCodeAsync(SendIWalletOtpCodeCommand request, CancellationToken cancellationToken)
    {
        switch (request.Type)
        {
            case 1:
                return await SendReturnOptSmsAsync(request);
            default:
                throw new NotImplementedException();
        }
    }

    private async Task<SendIWalletOtpCodeResponseDto> SendReturnOptSmsAsync(SendIWalletOtpCodeCommand request)
    {
        var phoneNumber = await _iWalletCardService.GetPhoneNumberAsync(request.WalletNumber);

        var smsRequest = new SendSms
        {
            TemplateName = "IWalletReturnOtpCode",
            TemplateParameters = new Dictionary<string, string>
            {
                {"Amount", request.Amount },
                {"MerchantName", request.MerchantName },
                {"OtpPassword", request.OtpPassword },


            },
            To = phoneNumber,
        };

        await _smsSenderService.SendSmsAsync(smsRequest);
        return new SendIWalletOtpCodeResponseDto();
    }
}
