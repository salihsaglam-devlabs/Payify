using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Response;

public class PosnetPostAuthResponse : PosnetResponseBase
{
    public PosnetPostAuthResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        Approved = responseXml.Element("approved")?.Value;
        ErrorCode = responseXml.Element("respCode")?.Value;
        ErrorMessage = responseXml.Element("respText")?.Value;
        HostLogKey = responseXml.Element("hostlogkey")?.Value;
        AuthCode = responseXml.Element("authCode")?.Value;
        InstallmentCount = responseXml.Descendants().FirstOrDefault(node => node.Name == "inst1")?.Value;
        InstAmount = responseXml.Descendants().FirstOrDefault(node => node.Name == "amnt1")?.Value;
        Point = responseXml.Descendants().FirstOrDefault(node => node.Name == "point")?.Value;
        PointAmount = responseXml.Descendants().FirstOrDefault(node => node.Name == "pointAmount")?.Value;
        TotalPoint = responseXml.Descendants().FirstOrDefault(node => node.Name == "totalPoint")?.Value;
        TotalPointAmount = responseXml.Descendants().FirstOrDefault(node => node.Name == "totalPointAmount")?.Value;

        return this;
    }
}
