using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;

public class VakifVoidRequest : VakifRequestBase
{
    public string TransactionType { get; set; }
    public string MerchantType { get; set; }
    public string HostSubMerchantId { get; set; }
    public string ReferenceTransactionId { get; set; }
    public string ClientIp { get; set; }

    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<VposRequest>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<Password>{Password.Trim()}</Password>");
        requestXml.AppendLine($"<TransactionType>{TransactionType.Trim()}</TransactionType>");
        requestXml.AppendLine($"<MerchantType>{MerchantType}</MerchantType>");
        if (IsTopUpPayment != true)
        {
            requestXml.AppendLine($"<HostSubMerchantId>{HostSubMerchantId.Trim()}</HostSubMerchantId>");
        }
        requestXml.AppendLine($"<ReferenceTransactionId>{ReferenceTransactionId.Trim()}</ReferenceTransactionId>");
        requestXml.AppendLine($"<ClientIp>{ClientIp.Trim()}</ClientIp>");
        requestXml.AppendLine("</VposRequest>");
        return requestXml.ToString();
    }
}
