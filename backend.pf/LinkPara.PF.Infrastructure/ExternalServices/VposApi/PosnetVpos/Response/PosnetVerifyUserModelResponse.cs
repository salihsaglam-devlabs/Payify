using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Response
{
    public class PosnetVerifyUserModelResponse : PosnetResponseBase
    {
        public string TxStatus { get; set; }
        public string MdStatus{ get; set; }
        public string MdErrorMessage { get; set; }
        public string Mac { get; set; }
        public PosnetVerifyUserModelResponse Parse(string response)
        {
            var responseXml = XElement.Parse(response);

            Approved = responseXml.Element("approved")?.Value;
            ErrorCode = responseXml.Element("respCode")?.Value;
            ErrorMessage = responseXml.Element("respText")?.Value;
            OrderNumber = responseXml.Descendants().FirstOrDefault(node => node.Name == "xid")?.Value;
            InstAmount = responseXml.Descendants().FirstOrDefault(node => node.Name == "amount")?.Value;
            CurrencyCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "currency")?.Value;
            InstallmentCount = responseXml.Descendants().FirstOrDefault(node => node.Name == "installment")?.Value;
            Point = responseXml.Descendants().FirstOrDefault(node => node.Name == "point")?.Value;
            PointAmount = responseXml.Descendants().FirstOrDefault(node => node.Name == "pointAmount")?.Value;
            TxStatus = responseXml.Descendants().FirstOrDefault(node => node.Name == "txStatus")?.Value;
            MdStatus = responseXml.Descendants().FirstOrDefault(node => node.Name == "mdStatus")?.Value;
            MdErrorMessage = responseXml.Descendants().FirstOrDefault(node => node.Name == "mdErrorMessage")?.Value;
            Mac = responseXml.Descendants().FirstOrDefault(node => node.Name == "mac")?.Value;

            return this;
        }
    }
}
