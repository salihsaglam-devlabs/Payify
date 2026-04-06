using LinkPara.HttpProviders.IKS.Models.Enums;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.IKS.Application.Commons.Mappings;
using LinkPara.IKS.Application.Features.Merchant.Command.UpdateMerchant;

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Request
{
    public class UpdateMerchantRequest : IMapFrom<UpdateMerchantCommand>
    {
        public Guid MerchantId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public string StatusCode { get; set; }
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
        public string AgreementDate { get; set; }
        public string Phone { get; set; }
        public string PspFlag { get; set; }
        public string MainSellerFlag { get; set; }
        public string MainSellerTaxNo { get; set; }
        public string ZipCode { get; set; }
        public string TaxOfficeName { get; set; }
        public string CommercialType { get; set; }
        public string TerminationCode { get; set; }
        public string EstablishmentDate { get; set; }
        public string BusinessModel { get; set; }
        public string BusinessActivity { get; set; }
        public int BranchCount { get; set; }
        public int EmployeeCount { get; set; }
        public string ManagerBirthDate { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public string WebsiteUrl { get; set; }
        public List<MerchantBusinessPartnerRequest> Partners { get; set; }
    }
}
