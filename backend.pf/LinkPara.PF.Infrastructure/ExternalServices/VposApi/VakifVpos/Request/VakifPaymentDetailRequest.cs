using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;

public class VakifPaymentDetailRequest : VakifRequestBase
{
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string TransactionId { get; set; }
    public string OrderId { get; set; }
    public string AuthCode { get; set; }
    public string MerchantType { get; set; }

    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<SearchRequest>");
        requestXml.AppendLine("<MerchantCriteria>");
        requestXml.AppendLine($"<MerchantPassword>{Password.Trim()}</MerchantPassword>");
        requestXml.AppendLine($"<MerchantType>{MerchantType}</MerchantType>");
        requestXml.AppendLine($"<HostMerchantId>{MerchantId.Trim()}</HostMerchantId>");
        requestXml.AppendLine("</MerchantCriteria>");
        requestXml.AppendLine("<TransactionCriteria>");
        requestXml.AppendLine($"<TransactionId>{TransactionId.Trim()}</TransactionId>");
        requestXml.AppendLine("</TransactionCriteria>");
        requestXml.AppendLine("</SearchRequest>");
        return requestXml.ToString();
    }
}
