using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
public class FinansPaymentDetailRequest : FinansPaymentBase
{
    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
        requestXml.AppendLine("<PayForRequest>");
        requestXml.AppendLine($"<MbrId>{MbrId}</MbrId>");
        requestXml.AppendLine($"<MerchantID>{MerchantId.Trim()}</MerchantID>");
        requestXml.AppendLine($"<UserCode>{UserCode.Trim()}</UserCode>");
        requestXml.AppendLine($"<UserPass>{UserPass}</UserPass>");
        requestXml.AppendLine($"<SecureType>{SecureType}</SecureType>");
        requestXml.AppendLine($"<TxnType>{TxnType}</TxnType>");
        requestXml.AppendLine($"<OrderId>{OrderId}</OrderId>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine($"<Lang>{LanguageCode}</Lang>");
        requestXml.AppendLine("</PayForRequest>");
        return requestXml.ToString();
    }
}
