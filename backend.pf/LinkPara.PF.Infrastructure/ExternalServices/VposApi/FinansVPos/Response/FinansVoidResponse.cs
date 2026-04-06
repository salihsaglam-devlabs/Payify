using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Response;
public class FinansVoidResponse : FinansResponseBase
{
    public FinansVoidResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        ProcReturnCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "ProcReturnCode")?.Value;
        HostRefNum = responseXml.Descendants().FirstOrDefault(node => node.Name == "HostRefNum")?.Value;
        AuthCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "AuthCode")?.Value;
        TxnResult = responseXml.Descendants().FirstOrDefault(node => node.Name == "TxnResult")?.Value;
        ErrorMessage = responseXml.Descendants().FirstOrDefault(node => node.Name == "ErrorMessage")?.Value;
        OrderId = responseXml.Descendants().FirstOrDefault(node => node.Name == "TransId")?.Value;

        return this;
    }
}
