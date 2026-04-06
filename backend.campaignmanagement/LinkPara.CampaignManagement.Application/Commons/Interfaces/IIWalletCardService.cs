
using LinkPara.CampaignManagement.Application.Features.IWalletCards;
using LinkPara.CampaignManagement.Application.Features.IWalletCards.Commands;
using LinkPara.CampaignManagement.Application.Features.IWalletCards.Queries.GetCard;
using LinkPara.CampaignManagement.Domain.Entities;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IIWalletCardService
{
    Task<IWalletCard> CreateCardAsync(CreateCardCommand request);
    Task<string> GetPhoneNumberAsync(string walletNumber);
    Task<IWalletCardDto> GetUserCardAsync(GetCardQuery request);
}
