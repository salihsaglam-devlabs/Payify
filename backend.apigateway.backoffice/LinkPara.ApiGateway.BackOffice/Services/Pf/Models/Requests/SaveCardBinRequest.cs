using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class SaveCardBinRequest
{
    public string BinNumber { get; set; }
    public CardBrand CardBrand { get; set; }
    public CardType? CardType { get; set; }
    public CardSubType? CardSubType { get; set; }
    public CardNetwork? CardNetwork { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public bool IsVirtual { get; set; }
    public int BankCode { get; set; }
}
