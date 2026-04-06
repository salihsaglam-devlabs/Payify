using System.Text;
using System.Text.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Request;

public class VakifInsuranceVoidRequest
{
    public VakifInsuranceVoidInnerRequest VposRequest { get; set; }
    
    public Dictionary<string, string> BuildRequest()
    {
        var json = JsonSerializer.Serialize(VposRequest);

        return new Dictionary<string, string>
        {
            { "VposRequest", json }
        };
    }
}

public class VakifInsuranceVoidInnerRequest : VakifInsurancePaymentBase
{
    public string ReferenceTransactionId { get; set; }
    public string HostSubMerchantId { get; set; }
    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<VposRequest>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<Password>{Password.Trim()}</Password>");
        requestXml.AppendLine($"<TransactionType>{TransactionType.Trim()}</TransactionType>");
        requestXml.AppendLine($"<MerchantType>{MerchantType}</MerchantType>");
        requestXml.AppendLine($"<HostSubMerchantId>{HostSubMerchantId.Trim()}</HostSubMerchantId>");
        requestXml.AppendLine($"<ReferenceTransactionId>{ReferenceTransactionId.Trim()}</ReferenceTransactionId>");
        requestXml.AppendLine($"<ClientIp>{ClientIp.Trim()}</ClientIp>");
        requestXml.AppendLine("</VposRequest>");
        return requestXml.ToString();
    }
}