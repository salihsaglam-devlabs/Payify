using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    public class PosnetVoidRequest : PosnetRequestBase
    {
        public string ReverseType { get; set; }
        public string BuildRequest()
        {
            var requestXml = new StringBuilder();
            requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestXml.AppendLine("<posnetRequest>");
            requestXml.AppendLine($"<mid>{MerchantId.Trim()}</mid>");
            requestXml.AppendLine($"<tid>{TerminalId.Trim()}</tid>");
            requestXml.AppendLine($"<reverse>");
            requestXml.AppendLine($"<transaction>{ReverseType}</transaction>");           
            requestXml.AppendLine($"<orderID>{OrderId}</orderID>"); 
            requestXml.AppendLine($"<orderDate>{OrderDate}</orderDate>");
            requestXml.AppendLine($"</reverse>");
            requestXml.AppendLine("</posnetRequest>");
            return requestXml.ToString();
        }
    }
}
