using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Response;

public class VakifInsuranceVoidResponse : VakifInsuranceResponseBase
{
    public string TransactionType { get; set; }
    public string ReferenceTransactionId { get; set; }
    public string TerminalNo { get; set; }
    public decimal TotalPoint { get; set; }
    public decimal CurrencyAmount { get; set; }
    public int CurrencyCode { get; set; }
    public string InstallmentCount { get; set; }
    public string OrderId { get; set; }
    public int ThreeDSecureType { get; set; }
    public int TransactionDeviceType { get; set; }
    public int BatchNo { get; set; }
    public decimal TlAmount { get; set; }
    public int MerchantType { get; set; }
    public string HostResultCode { get; set; }
    
    public VakifInsuranceVoidResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        TransactionId = responseXml.Element("TransactionId")?.Value;
        MerchantId = responseXml.Element("MerchantId")?.Value;
        ResultCode = responseXml.Element("ResultCode")?.Value;
        AuthCode = responseXml.Element("AuthCode")?.Value;
        ResultDetail = responseXml.Element("ResultDetail")?.Value;
        Rrn = responseXml.Element("Rrn")?.Value;

        if (!string.IsNullOrEmpty(responseXml.Element("HostDate")?.Value))
            HostDate = responseXml.Element("HostDate")?.Value;

        return this;
    }
}