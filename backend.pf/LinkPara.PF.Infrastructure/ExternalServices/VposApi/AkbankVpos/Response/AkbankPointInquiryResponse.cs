using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Response;

public class AkbankPointInquiryResponse : AkbankResponseBase
{
    public decimal AvailablePoint { get; set; }
    public AkbankPointInquiryResponse Parse(string response)
    {
        var akbankPointInquiryResponse = JObject.Parse(response);

        var points = akbankPointInquiryResponse.GetValue("reward");

        TxnCode = akbankPointInquiryResponse["txnCode"]?.ToString();
        ResponseCode = akbankPointInquiryResponse["responseCode"]?.ToString();
        ResponseMessage = akbankPointInquiryResponse["responseMessage"]?.ToString();
        HostResponseCode = akbankPointInquiryResponse["hostResponseCode"]?.ToString();
        HostMessage = akbankPointInquiryResponse["hostMessage"]?.ToString();
        TxnDateTime = DateTime.Parse(akbankPointInquiryResponse["txnDateTime"]?.ToString());
        AvailablePoint = decimal.Parse(points["ccbBalanceRewardAmount"]?.ToString());

        return this;
    }
}
