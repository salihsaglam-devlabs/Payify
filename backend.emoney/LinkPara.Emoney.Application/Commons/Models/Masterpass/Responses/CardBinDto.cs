using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Features.Banks;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;

public class CardBinDto
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
