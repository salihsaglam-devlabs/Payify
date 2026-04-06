using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Request;

public class VakifInsurancePaymentDetailRequest : VakifInsuranceRequestBase
{
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string TransactionId { get; set; }
    public string OrderId { get; set; }
    public string AuthCode { get; set; }
    public string MerchantType { get; set; }

    public Dictionary<string, string> BuildRequest()
    {
        var formData = new Dictionary<string, string>
        {
            { "MerchantPassword", Password },
            { "MerchantType", MerchantType },
            { "HostMerchantId", MerchantId }
        };

        if (!string.IsNullOrWhiteSpace(TransactionId))
        {
            formData.Add("TransactionId", TransactionId);
        }

        if (!string.IsNullOrWhiteSpace(OrderId))
        {
            formData.Add("OrderId", OrderId);
        }

        if (!string.IsNullOrWhiteSpace(AuthCode))
        {
            formData.Add("AuthCode", AuthCode);
        }

        if (!string.IsNullOrWhiteSpace(StartDate))
        {
            formData.Add("StartDate", StartDate);
        }

        if (!string.IsNullOrWhiteSpace(EndDate))
        {
            formData.Add("EndDate", EndDate);
        }

        return formData;
    }
}