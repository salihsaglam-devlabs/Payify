using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Request;

public class KuveytPaymentNonSecureRequest : KuveytPaymentBase
{
    public string BuildRequest()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<KuveytTurkVPosMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" \r\nxmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
        requestXml.AppendLine($"<APIVersion>{APIVersion.Trim()}</APIVersion>");
        requestXml.AppendLine($"<HashData>{HashData.Trim()}</HashData>");
        requestXml.AppendLine($"<HashPassword>{HashPassword.Trim()}</HashPassword>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<CustomerId>{CustomerId.Trim()}</CustomerId>");
        requestXml.AppendLine($"<UserName>{UserName}</UserName>");
        requestXml.AppendLine($"<CardNumber>{Pan.Trim()}</CardNumber>");
        requestXml.AppendLine($"<CardExpireDateYear>{ExpireYear}</CardExpireDateYear>");
        requestXml.AppendLine($"<CardExpireDateMonth>{ExpireMonth}</CardExpireDateMonth>");
        requestXml.AppendLine($"<CardCVV2>{Cvv2.Trim()}</CardCVV2>");
        requestXml.AppendLine($"<CardHolderName>{CardHolderName.Trim()}</CardHolderName>");
        requestXml.AppendLine($"<Amount>{Amount}</Amount>");
        requestXml.AppendLine($"<DisplayAmount>{Amount}</DisplayAmount>");
        requestXml.AppendLine($"<MerchantOrderId>{MerchantOrderId}</MerchantOrderId>");
        requestXml.AppendLine($"<CurrencyCode>{Currency}</CurrencyCode>");
        requestXml.AppendLine($"<TransactionSecurity>{TransactionSecurity.Trim()}</TransactionSecurity>");
        requestXml.AppendLine($"<TransactionType>{TransactionType.Trim()}</TransactionType>");
        requestXml.AppendLine($"<InstallmentCount>{InstallmentCount}</InstallmentCount>");
        requestXml.AppendLine("<PFSubMerchantData>");
        requestXml.AppendLine($"<PFSubMerchantId>{PFSubMerchantId.Trim()}</PFSubMerchantId>");
        requestXml.AppendLine($"<PFSubMerchantIdentityTaxNumber>{PFSubMerchantIdentityTaxNumber}</PFSubMerchantIdentityTaxNumber>");
        requestXml.AppendLine($"<PFSubMerchantTerminalId>{VposSubMerchantId}</PFSubMerchantTerminalId>");
        requestXml.AppendLine($"<BKMId>{BKMId}</BKMId>");
        requestXml.AppendLine("</PFSubMerchantData>");
        requestXml.AppendLine("</KuveytTurkVPosMessage>");

        return requestXml.ToString();
    }
}
