
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities
{
    public class Link : AuditEntity, ITrackChange
    {
        public string LinkCode { get; set; }
        public ChannelStatus LinkStatus { get; set; }
        public ChannelPaymentStatus LinkPaymentStatus { get; set; }
        public LinkType LinkType { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int CurrentUsageCount { get; set; }
        public int MaxUsageCount  { get; set; }
        public string OrderId { get; set; }
        public LinkAmountType LinkAmountType { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public bool CommissionFromCustomer { get; set; }
        public bool Is3dRequired { get; set; }
        public Guid MerchantId { get; set; }
        public string MerchantName { get; set; }
        public string MerchantNumber { get; set; }
        public Guid? SubMerchantId { get; set; }
        public string SubMerchantName { get; set; }
        public string SubMerchantNumber { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ReturnUrl { get; set; }
        public bool IsNameRequired { get; set; }
        public bool IsEmailRequired { get; set; }
        public bool IsPhoneNumberRequired { get; set; }
        public bool IsAddressRequired { get; set; }
        public bool IsNoteRequired { get; set; }
        public List<LinkInstallment> LinkInstallments { get; set; }

    }
}
