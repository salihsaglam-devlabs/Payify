using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Request;

public class KuveytPayment3dModelRequest : KuveytPaymentBase
{
    public string MD { get; set; }
    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<KuveytTurkVPosMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\nxmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
        requestXml.AppendLine($"<APIVersion>{APIVersion.Trim()}</APIVersion>");
        requestXml.AppendLine($"<HashData>{HashData.Trim()}</HashData>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<CustomerId>{CustomerId.Trim()}</CustomerId>");
        requestXml.AppendLine($"<UserName>{UserName}</UserName>");
        requestXml.AppendLine($"<TransactionType>{TransactionType.Trim()}</TransactionType>");
        requestXml.AppendLine($"<InstallmentCount>{InstallmentCount}</InstallmentCount>");
        requestXml.AppendLine($"<Amount>{Amount}</Amount>");
        requestXml.AppendLine($"<MerchantOrderId>{MerchantOrderId}</MerchantOrderId>");
        requestXml.AppendLine($"<TransactionSecurity>{TransactionSecurity.Trim()}</TransactionSecurity>");
        requestXml.AppendLine("<KuveytTurkVPosAdditionalData>");
        requestXml.AppendLine("<AdditionalData>");
        requestXml.AppendLine($"<Key>MD</Key>");
        requestXml.AppendLine($"<Data>{MD.Trim()}</Data>");
        requestXml.AppendLine("</AdditionalData>");
        requestXml.AppendLine("</KuveytTurkVPosAdditionalData>");
        requestXml.AppendLine("</KuveytTurkVPosMessage>");

        return requestXml.ToString();
    }
}
