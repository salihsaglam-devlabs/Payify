using System.Globalization;
using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;

public class VakifPaymentNonSecureRequest : VakifPaymentBase
{
    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<VposRequest>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<Password>{Password.Trim()}</Password>");
        requestXml.AppendLine($"<TerminalNo>{TerminalNo.Trim()}</TerminalNo>");
        requestXml.AppendLine($"<TransactionType>{TransactionType.Trim()}</TransactionType>");
        requestXml.AppendLine($"<TransactionId>{TransactionId.Trim()}</TransactionId>");
        requestXml.AppendLine($"<CurrencyAmount>{CurrencyAmount}</CurrencyAmount>");
        requestXml.AppendLine($"<CurrencyCode>{CurrencyCode}</CurrencyCode>");
        if (NumberOfInstallments > 1)
        {
            requestXml.AppendLine($"<NumberOfInstallments>{NumberOfInstallments}</NumberOfInstallments>");
        }
        requestXml.AppendLine($"<Pan>{Pan.Trim()}</Pan>");
        requestXml.AppendLine($"<Expiry>{Expiry}</Expiry>");
        requestXml.AppendLine($"<Cvv>{Cvv.Trim()}</Cvv>");
        requestXml.AppendLine($"<ClientIp>{ClientIp.Trim()}</ClientIp>");
        requestXml.AppendLine($"<HostSubMerchantId>{HostSubMerchantId.Trim()}</HostSubMerchantId>");
        requestXml.AppendLine($"<MerchantType>{MerchantType}</MerchantType>");
        requestXml.AppendLine($"<TransactionDeviceSource>{TransactionDeviceSource}</TransactionDeviceSource>");
        requestXml.AppendLine("</VposRequest>");
        return requestXml.ToString();
    }
}