namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Response;

public class IKSMerchantResponse
{
    public IKSMerchant merchant { get; set; }

}
public class IKSMerchant
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
    public string countryCode { get; set; }
    public int licenseTag { get; set; }
    public int mcc { get; set; }
    public string managerName { get; set; }
    public string agreementDate { get; set; }
    public string phone { get; set; }
    public string pspFlag { get; set; }
    public string mainSellerFlag { get; set; }
    public string mainSellerTaxNo { get; set; }
    public float latitude { get; set; }
    public float longitude { get; set; }
    public string zipCode { get; set; }
    public string taxOfficeName { get; set; }
    public string commercialType { get; set; }
    public string nationalAddressCode { get; set; }
    public Additionalinfo[] additionalInfo { get; set; }
}
public class Additionalinfo
{
    public string code { get; set; }
    public string description { get; set; }
}

