using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class GetChargebackRequest : SearchQueryParams
    {
        public Guid? TransactionId { get; set; }
        public TransactionType? TransactionType { get; set; }
        public string OrderId { get; set; }
        public string WalletNumber { get; set; }
        public string MerchantName { get; set; }
        public DateTime? TransactionDate { get; set; }
        public ChargebackStatus? Status { get; set; }
        public string UserName { get; set; }
    }
}
