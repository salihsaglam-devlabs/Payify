using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
public class FinansVoidRequest : FinansRequestBase
{
    public string OrgOrderId { get; set; }
    public string SecureType { get; set; }
    public string TxnType { get; set; }
    public string PurchAmount { get; set; }
    public int Currency { get; set; }
    public string Lang { get; set; }

    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<PayforRequest>");
        requestXml.AppendLine($"<MbrId>{MbrId.Trim()}</MbrId>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<UserCode>{UserCode.Trim()}</UserCode>");
        requestXml.AppendLine($"<UserPass>{UserPass.Trim()}</UserPass>");
        requestXml.AppendLine($"<OrderId>{OrgOrderId.Trim()}</OrderId>");
        requestXml.AppendLine($"<SecureType>{SecureType.Trim()}</SecureType>");
        requestXml.AppendLine($"<TxnType>{TxnType.Trim()}</TxnType>");
        requestXml.AppendLine($"<PurchAmount>{PurchAmount}</PurchAmount>");
        requestXml.AppendLine($"<Currency>{Currency}</Currency>");
        requestXml.AppendLine($"<Lang>{Lang.Trim()}</Lang>");
        requestXml.AppendLine("</PayforRequest>");
        return requestXml.ToString();
    }
}
