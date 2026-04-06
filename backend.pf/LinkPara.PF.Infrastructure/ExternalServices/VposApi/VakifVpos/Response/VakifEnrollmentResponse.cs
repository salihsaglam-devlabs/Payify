using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Response;

public class VakifEnrollmentResponse : VakifResponseBase
{
    public string Status { get; set; }
    public string MessageID { get; set; }
    public string Version { get; set; }
    public string PAReq { get; set; }
    public string ACSUrl { get; set; }
    public string TermURL { get; set; }
    public string MD { get; set; }
    public string ActualBrand { get; set; }
    public string MessageErrorCode { get; set; }

    public VakifEnrollmentResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        Status = responseXml.Descendants().FirstOrDefault(node => node.Name == "Status")?.Value;
        Version = responseXml.Descendants().FirstOrDefault(node => node.Name == "Version")?.Value;
        PAReq = responseXml.Descendants().FirstOrDefault(node => node.Name == "PaReq")?.Value;
        ACSUrl = responseXml.Descendants().FirstOrDefault(node => node.Name == "ACSUrl")?.Value;
        TermURL = responseXml.Descendants().FirstOrDefault(node => node.Name == "TermUrl")?.Value;
        MD = responseXml.Descendants().FirstOrDefault(node => node.Name == "MD")?.Value;
        ActualBrand = responseXml.Descendants().FirstOrDefault(node => node.Name == "ACTUALBRAND")?.Value;
        MessageErrorCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "MessageErrorCode")?.Value;

        return this;
    }
}
