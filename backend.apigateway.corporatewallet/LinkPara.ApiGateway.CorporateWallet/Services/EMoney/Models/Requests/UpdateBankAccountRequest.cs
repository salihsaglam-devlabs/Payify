using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class UpdateBankAccountRequest
{
    public string Tag { get; set; }
    public string Iban { get; set; }
}

public class UpdateBankAccountServiceRequest : UpdateBankAccountRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
