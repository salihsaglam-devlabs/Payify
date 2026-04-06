using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Models.Responses;
using MediatR;

namespace LinkPara.CampaignManagement.Application.Features.Campaigns.Queries.GetCampaigns;

public class GetCampaignsQuery : IRequest<List<CampaignDto>>
{
}

public class GetCampaignsQueryHandler : IRequestHandler<GetCampaignsQuery, List<CampaignDto>>
{
    private readonly ICampaignService _campaignService;

    public GetCampaignsQueryHandler(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    public async Task<List<CampaignDto>> Handle(GetCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _campaignService.GetAllCampaignFromExternalAsync();
        return Map(campaigns);
    }

    private List<CampaignDto> Map(List<Campaign> campaigns)
    {
        var result = new List<CampaignDto>();
        foreach (var item in campaigns)
        {
            var campaign = new CampaignDto
            {
                Body = item.body,
                CampaignType = item.campaign_type,
                CbType = item.cb_type,
                EndDate = item.end_date,
                Id = item.id,
                ImageUrl = item.image_url,
                MinAmount = item.min_amount,
                StartDate = item.start_date,
                Title = item.title,
                CampaignMerchants = new List<CampaignMerchantDto>()
            };
            var campaignMerchants = new List<CampaignMerchantDto>();
            item.campaign_merchants.ForEach(campaign_merchant =>
            {
                var campaignMerchant = new CampaignMerchantDto
                {
                    Description = $"{campaign_merchant.merchant.name} harcamalarında anında %{campaign_merchant.cb_amount} iade.",
                    CbAmount = campaign_merchant.cb_amount,
                    Content = campaign_merchant.content,
                    ImageUrl = campaign_merchant.image_url,
                };
                var merchant = new MerchantDto
                {
                    Id = campaign_merchant.merchant.id,
                    Logo = campaign_merchant.merchant.logo,
                    Name = campaign_merchant.merchant.name,
                    SectorList = campaign_merchant.merchant.sector_arr,
                    Type = campaign_merchant.merchant._type
                };

                var sectors = new List<SectorDto>();
                campaign_merchant.merchant.sector_array.ForEach(sector =>
                {
                    sectors.Add(new SectorDto
                    {
                        Id = sector.id,
                        Letter = sector.letter,
                        Name = sector.name,
                    });
                });

                merchant.Sectors = sectors;

                campaignMerchant.Merchant = merchant;

                campaignMerchants.Add(campaignMerchant);

            });
            campaign.CampaignMerchants.AddRange(campaignMerchants);
            result.Add(campaign);
        }
        return result;
    }
}
