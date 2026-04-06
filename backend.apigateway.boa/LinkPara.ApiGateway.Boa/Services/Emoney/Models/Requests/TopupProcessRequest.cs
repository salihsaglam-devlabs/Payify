using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class TopupProcessRequest
{
    public decimal Amount { get; set; }
    public string WalletNumber { get; set; }
    public string Currency { get; set; }
    public Guid CardTopupRequestId { get; set; }
    public string CardHolderName { get; set; }
    public string CardToken { get; set; }
    public string Description { get; set; }
}

public class TopupProcessServiceRequest : TopupProcessRequest, IHasUserId
{
    public Guid UserId { get; set; }
}