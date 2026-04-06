using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    public class PosnetPaymentDetailRequest : PosnetPaymentBase
    {
        public string BuildRequest()
        {
            var requestXml = new StringBuilder();
            requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestXml.AppendLine("<posnetRequest>");
            requestXml.AppendLine($"<mid>{MerchantId.Trim()}</mid>");
            requestXml.AppendLine($"<tid>{TerminalId.Trim()}</tid>");
            requestXml.AppendLine($"<agreement>");
            requestXml.AppendLine($"<orderID>{OrderId}</orderID>");
            requestXml.AppendLine($"<orderDate>{OrderDate}</orderDate>"); 
            requestXml.AppendLine($"</agreement>");
            requestXml.AppendLine("</posnetRequest>");
            return requestXml.ToString();
        }
    }
}
