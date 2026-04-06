using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;

public class VakifPostAuthRequest : VakifRequestBase
{
    public string TransactionType { get; set; }
    public string CurrencyAmount { get; set; }
    public int CurrencyCode { get; set; }   
    public string ClientIp { get; set; }
    public string ReferenceTransactionId { get; set; }
    public string HostSubMerchantId { get; set; }
    public string TransactionId { get; set; }
    public string MerchantType { get; set; }

    public string BuildRequest()
    {
        var requestXml = new StringBuilder(); requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<VposRequest>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<Password>{Password.Trim()}</Password>");
        requestXml.AppendLine($"<TerminalNo>{TerminalNo.Trim()}</TerminalNo>");
        requestXml.AppendLine($"<TransactionId>{TransactionId.Trim()}</TransactionId>");
        requestXml.AppendLine($"<TransactionType>{TransactionType.Trim()}</TransactionType>");
        requestXml.AppendLine($"<CurrencyAmount>{CurrencyAmount}</CurrencyAmount>");
        requestXml.AppendLine($"<CurrencyCode>{CurrencyCode}</CurrencyCode>");
        requestXml.AppendLine($"<ClientIp>{ClientIp.Trim()}</ClientIp>");
        requestXml.AppendLine($"<ReferenceTransactionId>{ReferenceTransactionId.Trim()}</ReferenceTransactionId>");
        requestXml.AppendLine($"<MerchantType>{MerchantType}</MerchantType>");
        requestXml.AppendLine($"<HostSubMerchantId>{HostSubMerchantId}</HostSubMerchantId>");
        requestXml.AppendLine("</VposRequest>"); return requestXml.ToString();
    }
}
