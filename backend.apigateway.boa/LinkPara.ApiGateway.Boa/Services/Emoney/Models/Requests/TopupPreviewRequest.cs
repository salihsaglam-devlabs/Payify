using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class TopupPreviewRequest
{
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
    public string WalletNumber { get; set; }
    public string Currency { get; set; }
}

public class TopupPreviewServiceRequest : TopupPreviewRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
