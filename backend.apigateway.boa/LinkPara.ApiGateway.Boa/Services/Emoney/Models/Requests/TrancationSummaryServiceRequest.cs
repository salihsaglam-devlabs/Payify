using LinkPara.ApiGateway.Boa.Commons.Helpers;

namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests
{
    public class TrancationSummaryServiceRequest : TransactionSummaryRequest, IHasUserId
    {
        public Guid UserId { get; set; }
    }

}
