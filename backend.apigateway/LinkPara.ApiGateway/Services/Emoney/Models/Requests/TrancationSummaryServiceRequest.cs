using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests
{
    public class TrancationSummaryServiceRequest : TransactionSummaryRequest, IHasUserId
    {
        public Guid UserId { get; set; }
    }

}
