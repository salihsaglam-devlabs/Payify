using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Response;

public class VakifInsuranceRefundResponse : VakifInsuranceResponseBase
{
    public VakifInsuranceRefundResponse Parse(string response)
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