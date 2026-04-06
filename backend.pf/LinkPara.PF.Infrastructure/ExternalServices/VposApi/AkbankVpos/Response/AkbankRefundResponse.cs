using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Response;

public class AkbankRefundResponse : AkbankResponseBase
{
    public AkbankRefundResponse Parse(string response)
    {
        var akbankRefundResponse = JObject.Parse(response);

        var order = akbankRefundResponse.GetValue("order");

        var transaction = akbankRefundResponse.GetValue("transaction");

        TxnCode = akbankRefundResponse["txnCode"]?.ToString();
        ResponseCode = akbankRefundResponse["responseCode"]?.ToString();
        ResponseMessage = akbankRefundResponse["responseMessage"]?.ToString();
        HostResponseCode = akbankRefundResponse["hostResponseCode"]?.ToString();
        HostMessage = akbankRefundResponse["hostMessage"]?.ToString();
        TxnDateTime = DateTime.Parse(akbankRefundResponse["txnDateTime"]?.ToString());
        OrderId = order["orderId"]?.ToString();
        RrnNumber = transaction["rrn"]?.ToString();
        AuthCode = transaction["authCode"]?.ToString();

        return this;
    }
}
