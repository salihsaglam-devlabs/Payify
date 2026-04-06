using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;

public class VakifPointInquiryRequest : VakifRequestBase
{
    public string TransactionType { get; set; }
    public string Pan { get; set; }
    public string Cvv { get; set; }
    public string Expiry { get; set; }
    public string ClientIp { get; set; }
    public string TransactionDeviceSource { get; set; }
    public string MerchantType { get; set; }
    public string HostSubMerchantId { get; set; }

    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<VposRequest>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<Password>{Password.Trim()}</Password>");
        requestXml.AppendLine($"<TerminalNo>{TerminalNo.Trim()}</TerminalNo>");
        requestXml.AppendLine($"<TransactionType>{TransactionType.Trim()}</TransactionType>");
        requestXml.AppendLine($"<Pan>{Pan.Trim()}</Pan>");
        requestXml.AppendLine($"<Cvv>{Cvv}</Cvv>");
        requestXml.AppendLine($"<Expiry>{Expiry}</Expiry>");
        requestXml.AppendLine($"<ClientIp>{ClientIp.Trim()}</ClientIp>");
        requestXml.AppendLine($"<HostSubMerchantId>{HostSubMerchantId.Trim()}</HostSubMerchantId>");
        requestXml.AppendLine($"<MerchantType>{MerchantType}</MerchantType>");
        requestXml.AppendLine($"<TransactionDeviceSource>{TransactionDeviceSource}</TransactionDeviceSource>");
        requestXml.AppendLine("</VposRequest>"); 
        return requestXml.ToString();
    }
}
