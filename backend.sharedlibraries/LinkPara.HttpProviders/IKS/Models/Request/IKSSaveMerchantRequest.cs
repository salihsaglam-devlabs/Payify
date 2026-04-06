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
    }
}
