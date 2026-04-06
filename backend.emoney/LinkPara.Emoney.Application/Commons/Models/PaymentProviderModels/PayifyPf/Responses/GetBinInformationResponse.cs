using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class GetBinInformationResponse : ResponseModel
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