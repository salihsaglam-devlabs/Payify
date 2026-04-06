using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
public class FinansPointInquiryRequest : FinansPaymentBase
{
    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<PayForRequest>");
        requestXml.AppendLine($"<MbrId>{MbrId.Trim()}</MbrId>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<UserCode>{UserCode.Trim()}</UserCode>");
        requestXml.AppendLine($"<UserPass>{UserPass.Trim()}</UserPass>");
        requestXml.AppendLine($"<OrderId>{OrderId.Trim()}</OrderId>");
        requestXml.AppendLine($"<SecureType>{SecureType.Trim()}</SecureType>");
        requestXml.AppendLine($"<TxnType>{TxnType.Trim()}</TxnType>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine($"<Pan>{Pan}</Pan>");
        requestXml.AppendLine($"<Lang>{LanguageCode.Trim()}</Lang>");
        requestXml.AppendLine("</PayForRequest>");
        return requestXml.ToString();
    }
}
