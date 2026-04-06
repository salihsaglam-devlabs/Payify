namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class InitChargebackRequest
    {
        public Guid TransactionId { get; set; }
        public string WalletNumber { get; set; }
        public string Description { get; set; }
        public string MerchantId { get; set; }
    }
}
