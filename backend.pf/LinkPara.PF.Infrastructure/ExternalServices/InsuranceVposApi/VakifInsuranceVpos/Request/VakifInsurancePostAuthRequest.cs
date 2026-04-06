namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Request;

public class VakifInsurancePostAuthRequest : VakifInsurancePaymentBase
{
    public string ReferenceTransactionId { get; set; }
    public string HostSubMerchantId { get; set; }
    public string MerchantType { get; set; }
}