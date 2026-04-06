using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Response;

public class PosnetPointInquiryResponse : PosnetResponseBase
{
    public PosnetPointInquiryResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        Approved = responseXml.Element("approved")?.Value;
        ErrorCode = responseXml.Element("respCode")?.Value;
        ErrorMessage = responseXml.Element("respText")?.Value;
        PointAmount = responseXml.Descendants().FirstOrDefault(node => node.Name == "pointAmount")?.Value;
        return this;
    }
}
