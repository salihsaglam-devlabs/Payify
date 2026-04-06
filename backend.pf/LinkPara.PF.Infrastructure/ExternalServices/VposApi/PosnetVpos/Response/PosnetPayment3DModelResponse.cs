using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Response;

public class PosnetPayment3DModelResponse : PosnetResponseBase
{
    public string Mac { get; set; }
    public PosnetPayment3DModelResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        Approved = responseXml.Element("approved")?.Value;
        ErrorCode = responseXml.Element("respCode")?.Value;
        ErrorMessage = responseXml.Element("respText")?.Value;
        HostLogKey = responseXml.Element("hostlogkey")?.Value;
        AuthCode = responseXml.Element("authCode")?.Value;

        return this;
    }
}
