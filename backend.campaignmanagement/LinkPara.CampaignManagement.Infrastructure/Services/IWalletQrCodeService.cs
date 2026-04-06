using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Interfaces.HttpClients;
using LinkPara.CampaignManagement.Application.Features.IWalletCards.Queries.GetCard;
using LinkPara.CampaignManagement.Application.Features.IWalletQrCodes;
using LinkPara.CampaignManagement.Domain.Entities;
using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class IWalletQrCodeService : IIWalletQrCodeService
{
    private readonly IIWalletHttpClient _httpClient;
    private readonly IGenericRepository<IWalletQrCode> _repository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IIWalletCardService _walletCardService;
    public IWalletQrCodeService(IIWalletHttpClient httpClient,
        IGenericRepository<IWalletQrCode> repository,
        IApplicationUserService applicationUserService,
        IIWalletCardService walletCardService)
    {
        _httpClient = httpClient;
        _repository = repository;
        _applicationUserService = applicationUserService;
        _walletCardService = walletCardService;
    }

    public async Task<IWalletQrCodeDto> GenerateQrCodeAsync(Guid userId, string walletNumber)
    {
        var card = await _walletCardService.GetUserCardAsync(new GetCardQuery { UserId = userId });
        
        if(card is null)
        {
            throw new NotFoundException(nameof(IWalletCard), userId);
        }

        var qrCode = new IWalletQrCode
        {
            UserId = userId,
            WalletNumber = walletNumber,
            CardId = card.CardId,
            CardNumber = card.CardNumber,
            IWalletQrCodeStatus = IWalletQrCodeStatus.Pending,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            IWalletCardId = card.Id
        };

        await _repository.AddAsync(qrCode);

        try
        {
            var qrCodeResponse = await _httpClient.GenerateQrCodeAsync(card.CardId);

            qrCode.Message = qrCodeResponse.message;
            qrCode.ExpiresIn = qrCodeResponse.Data.expires_in;
            qrCode.QrCode = qrCodeResponse.Data.qr_code;
            qrCode.IWalletQrCodeStatus = IWalletQrCodeStatus.Created;

            await _repository.UpdateAsync(qrCode);

            return new IWalletQrCodeDto
            {
                ExpiresIn = qrCode.ExpiresIn,
                QrCode = qrCode.QrCode,
            };

        }
        catch (Exception exception)
        {
            qrCode.Message = exception.Message;
            qrCode.IWalletQrCodeStatus = IWalletQrCodeStatus.Error;
            await _repository.UpdateAsync(qrCode);

            throw new InvalidOperationException();
        }
    }
}
