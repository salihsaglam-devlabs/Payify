using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Request;

public class KuveytInit3dModelRequest : KuveytPaymentBase
{
    public string ClientIpAddress { get; set; }
    public string SubmerchantCity { get; set; }
    public string SubmerchantCountry { get; set; }
    public string SubmerchantAddress { get; set; }
    public string SubmerchantPostalCode { get; set; }
    public string SubmerchantDistrict { get; set; }
    public string SubmerchantEmail { get; set; }
    public string SubmerchantPhoneCode { get; set; }
    public string SubmerchantPhoneNumber { get; set; }
    public string BuildRequest()
    {
        if (SubmerchantAddress.Length > 150)
        {
            SubmerchantAddress = SubmerchantAddress.Substring(0, 150);
        }
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<KuveytTurkVPosMessage xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" \r\nxmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">");
        requestXml.AppendLine($"<APIVersion>{APIVersion.Trim()}</APIVersion>");
        requestXml.AppendLine($"<OkUrl>{OkUrl.Trim()}</OkUrl>");
        requestXml.AppendLine($"<FailUrl>{FailUrl.Trim()}</FailUrl>");
        requestXml.AppendLine($"<HashData>{HashData.Trim()}</HashData>");
        requestXml.AppendLine($"<MerchantId>{MerchantId.Trim()}</MerchantId>");
        requestXml.AppendLine($"<CustomerId>{CustomerId.Trim()}</CustomerId>");
        requestXml.AppendLine($"<UserName>{UserName}</UserName>");
        requestXml.AppendLine($"<CardNumber>{Pan.Trim()}</CardNumber>");
        requestXml.AppendLine($"<CardExpireDateYear>{ExpireYear}</CardExpireDateYear>");
        requestXml.AppendLine($"<CardExpireDateMonth>{ExpireMonth}</CardExpireDateMonth>");
        requestXml.AppendLine($"<CardCVV2>{Cvv2.Trim()}</CardCVV2>");
        requestXml.AppendLine($"<CardHolderName>{CardHolderName.Trim()}</CardHolderName>");
        requestXml.AppendLine($"<TransactionType>{TransactionType.Trim()}</TransactionType>");
        requestXml.AppendLine($"<InstallmentCount>{InstallmentCount}</InstallmentCount>");
        requestXml.AppendLine($"<Amount>{Amount}</Amount>");
        requestXml.AppendLine($"<DisplayAmount>{Amount}</DisplayAmount>");
        requestXml.AppendLine($"<CurrencyCode>{Currency}</CurrencyCode>");
        requestXml.AppendLine($"<MerchantOrderId>{MerchantOrderId}</MerchantOrderId>");
        requestXml.AppendLine("<DeviceData>");
        requestXml.AppendLine($"<DeviceChannel>02</DeviceChannel>");
        requestXml.AppendLine($"<ClientIP>{ClientIpAddress.Trim()}</ClientIP>");
        requestXml.AppendLine("</DeviceData>");
        requestXml.AppendLine("<CardHolderData>");
        requestXml.AppendLine($"<BillAddrCity>{SubmerchantCity.Trim()}</BillAddrCity>");
        requestXml.AppendLine($"<BillAddrCountry>{SubmerchantCountry}</BillAddrCountry>");
        requestXml.AppendLine($"<BillAddrLine1>{SubmerchantAddress.Trim()}</BillAddrLine1>");
        requestXml.AppendLine($"<BillAddrPostCode>{SubmerchantPostalCode.Trim()}</BillAddrPostCode>");
        requestXml.AppendLine($"<BillAddrState>{SubmerchantDistrict}</BillAddrState>");
        requestXml.AppendLine($"<Email>{SubmerchantEmail.Trim()}</Email>");
        requestXml.AppendLine("<MobilePhone>");
        requestXml.AppendLine($"<Cc>{SubmerchantPhoneCode.Replace("+", "").Trim()}</Cc>");
        requestXml.AppendLine($"<Subscriber>{SubmerchantPhoneNumber.Trim()}</Subscriber>");
        requestXml.AppendLine("</MobilePhone>");
        requestXml.AppendLine("</CardHolderData>");
        requestXml.AppendLine($"<TransactionSecurity>{TransactionSecurity.Trim()}</TransactionSecurity>");
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
