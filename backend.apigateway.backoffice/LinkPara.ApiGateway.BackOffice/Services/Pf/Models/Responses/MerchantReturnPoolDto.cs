using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses
{
    public class MerchantReturnPoolDto
    {
        public Guid Id { get; set; }
        public DateTime ActionDate { get; set; }
        public Guid ActionUser { get; set; }
        public ReturnStatus ReturnStatus { get; set; } = ReturnStatus.Pending;
        public decimal Amount { get; set; }
        public string OrderId { get; set; }
        public string ConversationId { get; set; }
        public string ClientIpAddress { get; set; }
        public string LanguageCode { get; set; }
        public Guid MerchantId { get; set; }
        public MerchantDto Merchant { get; set; }
        public DateTime CreateDate { get; set; }
        public string CardNumber { get; set; }
        public int BankCode { get; set; }
        public string BankName { get; set; }
        public string RejectDescription { get; set; }
        public bool? BankStatus { get; set; }
        public string CurrencyCode { get; set; }
        public string BankResponseCode { get; set; }
        public string BankResponseDescription { get; set; }
    }
}
