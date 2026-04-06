using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Response;

public class AkbankPostAuthResponse : AkbankResponseBase
{
    public AkbankPostAuthResponse Parse(string response)
    {
        var akbankPostAuthResponse = JObject.Parse(response);

        var transaction = akbankPostAuthResponse.GetValue("transaction");

        var order = akbankPostAuthResponse.GetValue("order");

        TxnCode = akbankPostAuthResponse["txnCode"]?.ToString();
        ResponseCode = akbankPostAuthResponse["responseCode"]?.ToString();
        ResponseMessage = akbankPostAuthResponse["responseMessage"]?.ToString();
        HostResponseCode = akbankPostAuthResponse["hostResponseCode"]?.ToString();
        HostMessage = akbankPostAuthResponse["hostMessage"]?.ToString();
        TxnDateTime = DateTime.Parse(akbankPostAuthResponse["txnDateTime"]?.ToString());
        OrderId = order["orderId"]?.ToString();
        RrnNumber = transaction["rrn"]?.ToString();
        AuthCode = transaction["authCode"]?.ToString();

        return this;
    }
}
