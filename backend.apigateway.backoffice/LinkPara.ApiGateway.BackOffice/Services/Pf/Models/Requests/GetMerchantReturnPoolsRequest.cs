using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class GetMerchantReturnPoolsRequest : SearchQueryParams
    {
        public string ConversationId { get; set; }
        public Guid? MerchantId { get; set; }
        public DateTime? ActionDate { get; set; }
        public Guid? ActionUser { get; set; }
        public ReturnStatus? ReturnStatus { get; set; }
        public string OrderId { get; set; }
        public DateTime? CreateDateStart { get; set; }
        public DateTime? CreateDateEnd { get; set; }
        public string FirstCardNumber { get; set; }
        public string LastCardNumber { get; set; }
        public int? BankCode { get; set; }
        public string BankName { get; set; }
        public bool? BankStatus { get; set; }
    }
}
