using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class SaveBankAccountRequest
{
    public string Iban { get; set; }
    public string Tag { get; set; }
}

public class SaveBankAccountServiceRequest : SaveBankAccountRequest, IHasUserId
{
    public Guid UserId { get; set; }
}  
