using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Request
{
    public class PosnetPointInquiryRequest : PosnetPaymentBase
    {
        public string GetPointDetail {  get; set; }
        public string BuildRequest()
        {
            var requestXml = new StringBuilder();
            requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            requestXml.AppendLine("<posnetRequest>");
            requestXml.AppendLine($"<mid>{MerchantId.Trim()}</mid>");
            requestXml.AppendLine($"<tid>{TerminalId.Trim()}</tid>");
            requestXml.AppendLine($"<tranDateRequired>{TranDateRequired}</tranDateRequired>");
            requestXml.AppendLine($"<pointInquiry>");
            requestXml.AppendLine($"<ccno>{CardNo}</ccno>");
            requestXml.AppendLine($"<expDate>{ExpireDate}</expDate>");
            requestXml.AppendLine($"<getPointDetail>{GetPointDetail}</getPointDetail>");
            requestXml.AppendLine($"</pointInquiry>");
            requestXml.AppendLine("</posnetRequest>");
            return requestXml.ToString();
        }
    }
}
