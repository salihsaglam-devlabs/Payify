using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Response
{
    public class PosnetInit3DModelResponse : PosnetResponseBase
    {
        public string PosnetData { get; set; }
        public string PosnetData2 { get; set; }
        public string Digest { get; set; }
        public PosnetInit3DModelResponse Parse(string response)
        {
            var responseXml = XElement.Parse(response);

            Approved = responseXml.Element("approved")?.Value;
            ErrorCode = responseXml.Element("respCode")?.Value;
            ErrorMessage = responseXml.Element("respText")?.Value;
            PosnetData = responseXml.Descendants().FirstOrDefault(node => node.Name == "data1")?.Value; 
            PosnetData2 = responseXml.Descendants().FirstOrDefault(node => node.Name == "data2")?.Value;
            Digest = responseXml.Descendants().FirstOrDefault(node => node.Name == "sign")?.Value;

            return this;
        }
    }
}
