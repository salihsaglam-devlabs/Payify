using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Payments.Response;

public class GetBinInformationResponse : ResponseBase
{
    public string BinNumber { get; set; }
    public CardBrand CardBrand { get; set; }
    public CardNetwork CardNetwork { get; set; }
    public CardType CardType { get; set; }
    public CardSubType CardSubType { get; set; }
    public int BankCode { get; set; }
    public string BankName { get; set; }
    public bool IsVirtual { get; set; }
}