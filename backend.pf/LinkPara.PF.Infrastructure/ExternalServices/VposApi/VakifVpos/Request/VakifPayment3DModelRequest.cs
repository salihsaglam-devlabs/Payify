using System.Globalization;
using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;

public class VakifPayment3DModelRequest : VakifPaymentBase
{
    public string Eci { get; set; }
    public string Cavv { get; set; }
    public string MpiTransactionId { get; set; }
    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<VposRequest>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<Password>{Password.Trim()}</Password>");
        requestXml.AppendLine($"<Pan>{Pan.Trim()}</Pan>");
        requestXml.AppendLine($"<Expiry>{Expiry}</Expiry>");
        requestXml.AppendLine($"<CurrencyAmount>{CurrencyAmount}</CurrencyAmount>");
        requestXml.AppendLine($"<CurrencyCode>{CurrencyCode}</CurrencyCode>");
        if (NumberOfInstallments > 1)
        {
            requestXml.AppendLine($"<NumberOfInstallments>{NumberOfInstallments}</NumberOfInstallments>");
        }
        requestXml.AppendLine($"<TerminalNo>{TerminalNo.Trim()}</TerminalNo>");
        requestXml.AppendLine($"<TransactionType>{TransactionType.Trim()}</TransactionType>");
        requestXml.AppendLine($"<TransactionId>{TransactionId.Trim()}</TransactionId>");
        requestXml.AppendLine($"<ClientIp>{ClientIp.Trim()}</ClientIp>");
        requestXml.AppendLine($"<ECI>{Eci.Trim()}</ECI>");
        requestXml.AppendLine($"<CAVV>{Cavv.Trim()}</CAVV>");
        requestXml.AppendLine($"<MpiTransactionId>{MpiTransactionId.Trim()}</MpiTransactionId>");
        if (IsTopUpPayment != true)
        {
            requestXml.AppendLine($"<HostSubMerchantId>{HostSubMerchantId.Trim()}</HostSubMerchantId>");
        }
        requestXml.AppendLine($"<MerchantType>{MerchantType}</MerchantType>");
        requestXml.AppendLine($"<TransactionDeviceSource>{TransactionDeviceSource}</TransactionDeviceSource>");
        requestXml.AppendLine("</VposRequest>");
        return requestXml.ToString();
    }
}
