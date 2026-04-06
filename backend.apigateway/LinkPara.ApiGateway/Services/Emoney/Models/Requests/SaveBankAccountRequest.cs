using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class SaveBankAccountRequest
{
    public string Iban { get; set; }
    public string Tag { get; set; }
    public string ReceiverName { get; set; }
}

public class SaveBankAccountServiceRequest : SaveBankAccountRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
