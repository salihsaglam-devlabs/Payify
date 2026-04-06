
using LinkPara.CampaignManagement.Application.Commons.Models.Responses;
using LinkPara.CampaignManagement.Domain.Entities;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces.HttpClients;

public interface IIWalletHttpClient
{
    Task<List<Campaign>> GetCampaignsAsync();
    Task<List<Agreement>> GetAgreementsAsync();
    Task<List<Town>> GetTownsAsync(int cityId);
    Task<List<City>> GetCitiesAsync();
    Task<IWalletCard> CreateCardAsync(IWalletCard card);
    Task<QrCodeResponse> GenerateQrCodeAsync(int cardId);
}
