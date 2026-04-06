using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.Banks;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.CardBins;

public class CardBinDto : IMapFrom<CardBin>
{
    public Guid Id { get; set; }
    public string BinNumber { get; set; }
    public CardBrand CardBrand { get; set; }
    public CardType CardType { get; set; }
    public CardSubType CardSubType { get; set; }
    public CardNetwork CardNetwork { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public bool IsVirtual { get; set; }
    public int BankCode { get; set; }
    public BankDto Bank { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
