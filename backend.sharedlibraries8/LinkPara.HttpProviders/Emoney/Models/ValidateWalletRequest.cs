using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.HttpProviders.Emoney.Models;
public class ValidateWalletRequest
{
    public string WalletNumber { get; set; }
    public string CurrencyCode { get; set; }
}
