using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    public class PosnetPayment3DModelRequest : PosnetPaymentBase
    {
        public string EncryptionKey { get; set; }
        public string BankPacket { get; set; }
        public string BuildRequest()
        {
            string firstHash = VposHelper.GetSha256($"{EncryptionKey};{TerminalId}");
            string MAC = VposHelper.GetSha256($"{OrderId};{Amount};{CurrencyCode};{MerchantId};{firstHash}");

            var requestXml = new StringBuilder();
            requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestXml.AppendLine("<posnetRequest>");
            requestXml.AppendLine($"<mid>{MerchantId.Trim()}</mid>");
            requestXml.AppendLine($"<tid>{TerminalId.Trim()}</tid>");
            requestXml.AppendLine($"<oosTranData>");
            requestXml.AppendLine($"<bankData>{BankPacket}</bankData>");
            requestXml.AppendLine($"<wpAmount>0</wpAmount>");
            requestXml.AppendLine($"<mac>{MAC}</mac>");
            requestXml.AppendLine($"</oosTranData>");
            requestXml.AppendLine("</posnetRequest>");
            return requestXml.ToString();
        }
    }
}
