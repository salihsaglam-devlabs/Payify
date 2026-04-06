using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
public class FinansPayment3DModelRequest : FinansPaymentBase
{
    public string RequestGuid { get; set; }
    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<PayForRequest>");
        requestXml.AppendLine($"<UserCode>{UserCode.Trim()}</UserCode>");
        requestXml.AppendLine($"<UserPass>{UserPass.Trim()}</UserPass>");
        requestXml.AppendLine($"<SecureType>{SecureType.Trim()}</SecureType>");
        requestXml.AppendLine($"<OrderId>{OrderId.Trim()}</OrderId>");
        requestXml.AppendLine($"<RequestGuid>{RequestGuid.Trim()}</RequestGuid>");
        requestXml.AppendLine("</PayForRequest>");
        return requestXml.ToString();
    }
}
