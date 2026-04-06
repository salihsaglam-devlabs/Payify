using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class GetReceiptRequest
{
    public Guid TransactionId { get; set; }
    public string CustomerNumber { get; set; }
}

public class GetReceiptServiceRequest : GetReceiptRequest, IHasUserId
{
    public Guid UserId { get; set; }    
}
