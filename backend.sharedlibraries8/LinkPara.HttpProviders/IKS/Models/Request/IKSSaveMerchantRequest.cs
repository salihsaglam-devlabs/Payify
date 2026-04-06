using LinkPara.HttpProviders.IKS.Models.Enums;
using LinkPara.HttpProviders.PF.Models.Request;

namespace LinkPara.HttpProviders.IKS.Models.Request
{
    public class IKSSaveMerchantRequest
    {
        public Guid MerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public string TaxNo { get; set; }
        public string TradeName { get; set; }
        public string MerchantName { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        public string Neighborhood { get; set; }
        public int LicenseTag { get; set; }
        public string CountryCode { get; set; }
        public int Mcc { get; set; }
        public string ManagerName { get; set; }
        public string Phone { get; set; }
        public string ZipCode { get; set; }
        public string TaxOfficeName { get; set; }
        public string CommercialType { get; set; }
        public MainSellerFlag MainSellerFlag { get; set; }
        public string MainSellerTaxNo { get; set; }
        public string EstablishmentDate { get; set; }
        public string BusinessModel { get; set; }
        public string BusinessActivity { get; set; }
        public int BranchCount { get; set; }
        public int EmployeeCount { get; set; }
        public string ManagerBirthDate { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public string WebsiteUrl { get; set; }
        public List<MerchantBusinessPartnerRequest> MerchantBusinessPartners { get; set; }
    }
}
