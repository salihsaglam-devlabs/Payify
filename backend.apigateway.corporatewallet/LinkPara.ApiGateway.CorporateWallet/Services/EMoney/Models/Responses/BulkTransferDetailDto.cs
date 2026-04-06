using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses
{
    public class BulkTransferDetailDto
    {
        public Guid Id { get; set; }
        public Guid BulkTransferId { get; set; }
        public string FullName { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string Description { get; set; }
        public string Receiver { get; set; }
        public BulkTransferDetailStatus BulkTransferDetailStatus { get; set; }
        public Guid? TransactionId { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal BsmvAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal PricingAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
