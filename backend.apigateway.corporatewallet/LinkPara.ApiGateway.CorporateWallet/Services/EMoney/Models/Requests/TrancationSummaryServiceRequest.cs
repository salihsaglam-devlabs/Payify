using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests
{
    public class TrancationSummaryServiceRequest : TransactionSummaryRequest, IHasUserId
    {
        public Guid UserId { get; set; }
    }

}
