using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Request;

public class NestPaymentDetailRequest : NestPayBase
{
    public string OrderId { get; set; }
    public string OrderStatus { get; set; }
    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<CC5Request>");
        requestXml.AppendLine($"<Name>{MerchantName.Trim()}</Name>");
        requestXml.AppendLine($"<Password>{Password.Trim()}</Password>");
        requestXml.AppendLine($"<ClientId>{ClientId.Trim()}</ClientId>");
        requestXml.AppendLine($"<OrderId>{OrderId.Trim()}</OrderId>");
        requestXml.AppendLine("<Extra>");
        requestXml.AppendLine($"<ORDERSTATUS>{OrderStatus.Trim()}</ORDERSTATUS>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");
        return requestXml.ToString();
    }
}
