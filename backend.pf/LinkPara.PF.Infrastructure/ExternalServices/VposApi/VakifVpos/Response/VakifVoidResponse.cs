using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Response;

public class VakifVoidResponse : VakifResponseBase
{
    public VakifVoidResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        TransactionId = responseXml.Element("TransactionId")?.Value;
        MerchantId = responseXml.Element("MerchantId")?.Value;
        ResultCode = responseXml.Element("ResultCode")?.Value;
        AuthCode = responseXml.Element("AuthCode")?.Value;
        ResultDetail = responseXml.Element("ResultDetail")?.Value;
        Rrn = responseXml.Element("Rrn")?.Value;

        if (!string.IsNullOrEmpty(responseXml.Element("HostDate")?.Value))
            HostDate = HostDateToDatetime(responseXml.Element("HostDate")?.Value);

        return this;
    }
}
