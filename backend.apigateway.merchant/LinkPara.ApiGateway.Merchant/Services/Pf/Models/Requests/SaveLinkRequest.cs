
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests
{
    public class SaveLinkRequest
    {
        public Guid MerchantId { get; set; }
        public Guid? SubMerchantId { get; set; }
        public LinkType LinkType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int MaxUsageCount { get; set; }
        public string OrderId { get; set; }
        public LinkAmountType LinkAmountType { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public List<int> Installments { get; set; }
        public bool CommissionFromCustomer { get; set; }
        public bool Is3dRequired { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ReturnUrl { get; set; }
        public bool IsNameRequired { get; set; }
        public bool IsEmailRequired { get; set; }
        public bool IsPhoneNumberRequired { get; set; }
        public bool IsAddressRequired { get; set; }
        public bool IsNoteRequired { get; set; }
    }
}
