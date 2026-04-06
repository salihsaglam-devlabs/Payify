using System.Text;
using System.Text.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Request;

public class VakifInsuranceRefundRequest
{
    public VakifInsuranceRefundInnerRequest VposRequest { get; set; }
    
    public Dictionary<string, string> BuildRequest()
    {
        var json = JsonSerializer.Serialize(VposRequest);
        var formData = new Dictionary<string, string>
        {
            { "VposRequest", json }
        };

        return formData;
    }
}

public class VakifInsuranceRefundInnerRequest : VakifInsurancePaymentBase
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
        requestXml.AppendLine($"<CurrencyAmount>{CurrencyAmount}</CurrencyAmount>");
        requestXml.AppendLine($"<TransactionId>{TransactionId.Trim()}</TransactionId>");
        requestXml.AppendLine($"<ReferenceTransactionId>{ReferenceTransactionId.Trim()}</ReferenceTransactionId>");
        requestXml.AppendLine($"<MerchantType>{MerchantType}</MerchantType>");
        requestXml.AppendLine($"<HostSubMerchantId>{HostSubMerchantId.Trim()}</HostSubMerchantId>");
        requestXml.AppendLine($"<ClientIp>{ClientIp.Trim()}</ClientIp>");
        requestXml.AppendLine("</VposRequest>");
        return requestXml.ToString();
    }
}