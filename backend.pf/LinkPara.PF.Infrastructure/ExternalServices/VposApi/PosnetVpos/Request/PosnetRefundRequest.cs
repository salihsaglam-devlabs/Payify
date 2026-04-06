using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    public class PosnetRefundRequest : PosnetPaymentBase
    {
        public string BuildRequest()
        {
            var requestXml = new StringBuilder();
            requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestXml.AppendLine("<posnetRequest>");
            requestXml.AppendLine($"<mid>{MerchantId.Trim()}</mid>");
            requestXml.AppendLine($"<tid>{TerminalId.Trim()}</tid>");
            requestXml.AppendLine($"<tranDateRequired>{TranDateRequired}</tranDateRequired>");
            requestXml.AppendLine($"<return>");
            requestXml.AppendLine($"<amount>{Amount}</amount>");
            requestXml.AppendLine($"<currencyCode>{CurrencyCode}</currencyCode>");
            requestXml.AppendLine($"<orderID>{OrderId}</orderID>"); 
            requestXml.AppendLine($"<orderDate>{OrderDate}</orderDate>");
            requestXml.AppendLine($"</return>");
            requestXml.AppendLine("</posnetRequest>");
            return requestXml.ToString();
        }
    }
}
