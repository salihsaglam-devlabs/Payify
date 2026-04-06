using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Request;

public class VakifInsurancePaymentNonSecureRequest : VakifInsurancePaymentBase
{
    public string HostSubMerchantId { get; set; }
    public Dictionary<string, string> BuildRequest()
    {
        var formData = new Dictionary<string, string>
        {
            { "MerchantId", MerchantId },
            { "Password", Password },
            { "TerminalNo", TerminalNo },
            { "TransactionType", TransactionType },
            { "TransactionId", TransactionId },
            { "CurrencyAmount", CurrencyAmount },
            { "CurrencyCode", CurrencyCode.ToString() },
            { "InquiryValue", InquiryValue },
            { "CardNoFirst", CardNoFirst },
            { "CardNoLast", CardNoLast },
            { "ClientIp", ClientIp },
            { "MerchantType", MerchantType },
            { "TransactionDeviceSource", TransactionDeviceSource }
        };

        if (!string.IsNullOrWhiteSpace(HostSubMerchantId))
        {
            formData.Add("HostSubMerchantId", HostSubMerchantId);
        }

        if (!string.IsNullOrWhiteSpace(InstallmentCount))
        {
            formData.Add("InstallmentCount", InstallmentCount);
        }

        return formData;
    }
}