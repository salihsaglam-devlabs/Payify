using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    public class PosnetVerifyUserModelRequest : PosnetPaymentBase
    {
        public string EncryptionKey { get; set; }
        public string BankPacket { get; set; }
        public string MerchantPacket { get; set; }
        public string Sign { get; set; }
        public string BuildRequest()
        {
            var MAC = BuildMacCode();

            var requestXml = new StringBuilder();
            requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestXml.AppendLine("<posnetRequest>");
            requestXml.AppendLine($"<mid>{MerchantId.Trim()}</mid>");
            requestXml.AppendLine($"<tid>{TerminalId.Trim()}</tid>");
            requestXml.AppendLine($"<oosResolveMerchantData>");
            requestXml.AppendLine($"<bankData>{BankPacket}</bankData>");
            requestXml.AppendLine($"<merchantData>{MerchantPacket}</merchantData>");
            requestXml.AppendLine($"<sign>{Sign}</sign>");
            requestXml.AppendLine($"<mac>{MAC}</mac>");
            requestXml.AppendLine($"</oosResolveMerchantData>");
            requestXml.AppendLine("</posnetRequest>");
            return requestXml.ToString();
        }
        private string BuildMacCode()
        {
            string firstHash = VposHelper.GetSha256($"{EncryptionKey};{TerminalId}");
            string MAC = VposHelper.GetSha256($"{OrderId};{Amount};{CurrencyCode};{MerchantId};{firstHash}");
            return MAC;
        }
    }
}
