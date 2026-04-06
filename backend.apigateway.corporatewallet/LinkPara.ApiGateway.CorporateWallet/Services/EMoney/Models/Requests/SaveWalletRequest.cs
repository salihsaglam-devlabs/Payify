using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class SaveWalletRequest
{
    public string FriendlyName { get; set; }
    public string CurrencyCode { get; set; }
    public CurrencyType CurrencyType { get; set; }
    public bool IsMainWallet { get; set; }
}

public class SaveWalletServiceRequest : SaveWalletRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
