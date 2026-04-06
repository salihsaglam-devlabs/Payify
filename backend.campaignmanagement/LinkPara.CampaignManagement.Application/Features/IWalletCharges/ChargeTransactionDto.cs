using LinkPara.CampaignManagement.Application.Commons.Mappings;
using LinkPara.CampaignManagement.Domain.Entities;
using LinkPara.CampaignManagement.Domain.Enums;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCharges;

public class ChargeTransactionDto : IMapFrom<IWalletChargeTransaction>
{
    public string FullName { get; set; }
    public string WalletNumber { get; set; }
    public SourceCampaignType SourceCampaignType { get; set; }
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public Guid ProcessGuid { get; set; }
    public ChargeTransactionType ChargeTransactionType { get; set; }
    public string MerchantName { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<IWalletChargeTransaction, ChargeTransactionDto>()
            .ForMember(d => d.SourceCampaignType, opt => opt.MapFrom(s => s.IWalletCharge != null ? s.IWalletCharge.SourceCampaignType : SourceCampaignType.Iwallet))
            .ForMember(d => d.WalletNumber, opt => opt.MapFrom(s => s.IWalletCharge != null ? s.IWalletCharge.WalletNumber : string.Empty))
            .ForMember(d => d.TransactionDate, opt => opt.MapFrom(s => s.IWalletCharge != null ? s.IWalletCharge.CreateDate : default(DateTime)))
            .ForMember(d => d.ProcessGuid, opt => opt.MapFrom(s => s.IWalletCharge != null ? s.IWalletCharge.Id : default(Guid)))
            .ForMember(d => d.MerchantName, opt => opt.MapFrom(s => s.IWalletCharge != null ? s.IWalletCharge.MerchantName : default(string)))
            .ForMember(d => d.CardNumber, opt => opt.MapFrom(s => s.IWalletCharge.IWalletQrCode != null ? s.IWalletCharge.IWalletQrCode.CardNumber : string.Empty))
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.IWalletCharge.IWalletQrCode.IWalletCard != null ? s.IWalletCharge.IWalletQrCode.IWalletCard.FullName : string.Empty));
    }

}
