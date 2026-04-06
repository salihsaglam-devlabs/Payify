using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses
{
    public class DeductionTransactionDto
    {
        public Guid MerchantId { get; set; }
        public Guid PostingBalanceId { get; set; }
        public Guid MerchantDeductionId { get; set; }
        public DeductionType DeductionType { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
