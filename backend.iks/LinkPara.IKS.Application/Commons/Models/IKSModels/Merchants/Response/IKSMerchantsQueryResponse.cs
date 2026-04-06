

namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Response
{
    public class IKSMerchantsQueryResponse
    {
        public List<MerchantQuery> merchants { get; set; }
        public int totalCount { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }

        public class MerchantQuery
        {
            public string globalMerchantId { get; set; }
            public string pspMerchantId { get; set; }
            public string taxNo { get; set; }
            public string statusCode { get; set; }
            public string tradeName { get; set; }
            public string merchantName { get; set; }
            public string address { get; set; }
            public string district { get; set; }
            public string neighborhood { get; set; }
            public int licenseTag { get; set; }
            public string countryCode { get; set; }
            public string zipCode { get; set; }
            public int mcc { get; set; }
            public string managerName { get; set; }
            public string agreementDate { get; set; }
            public string phone { get; set; }
            public string pspFlag { get; set; }
            public string mainSellerFlag { get; set; }
            public float latitude { get; set; }
            public float longitude { get; set; }
            public string taxOfficeName { get; set; }
            public string commercialType { get; set; }
            public Geocodeinfo geocodeInfo { get; set; }
            public string nationalAddressCode { get; set; }
        }
        public class Geocodeinfo
        {
            public float latitude { get; set; }
            public float longitude { get; set; }
            public string scoreQuality { get; set; }
            public int scoreDepth { get; set; }
            public string suggestedAddress { get; set; }
        }
    }
}
