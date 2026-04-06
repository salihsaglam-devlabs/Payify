using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests
{
    public class SaveMerchantLimitRequest
    {
        public TransactionLimitType TransactionLimitType { get; set; }
        public Period Period { get; set; }
        public LimitType LimitType { get; set; }
        public int? MaxPiece { get; set; }
        public decimal? MaxAmount { get; set; }
        public Guid MerchantId { get; set; }
        public string Currency { get; set; }
    }
}
