using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Response;

public class AkbankPaymentNonSecureResponse : AkbankResponseBase
{
    public AkbankPaymentNonSecureResponse Parse(string response)
    {
        var akbankNonSecureResponse = JObject.Parse(response);

        var transaction = akbankNonSecureResponse.GetValue("transaction");

        var order = akbankNonSecureResponse.GetValue("order");

        TxnCode = akbankNonSecureResponse["txnCode"]?.ToString();
        ResponseCode = akbankNonSecureResponse["responseCode"]?.ToString();
        ResponseMessage = akbankNonSecureResponse["responseMessage"]?.ToString();
        HostResponseCode = akbankNonSecureResponse["hostResponseCode"]?.ToString();
        HostMessage = akbankNonSecureResponse["hostMessage"]?.ToString();
        TxnDateTime = DateTime.Parse(akbankNonSecureResponse["txnDateTime"]?.ToString());
        OrderId = order["orderId"]?.ToString();
        RrnNumber = transaction["rrn"]?.ToString();
        AuthCode = transaction["authCode"]?.ToString();

        return this;
    }
}
