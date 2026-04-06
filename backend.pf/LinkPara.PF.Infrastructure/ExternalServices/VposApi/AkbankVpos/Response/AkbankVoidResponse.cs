using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Response;

public class AkbankVoidResponse : AkbankResponseBase
{
    public AkbankVoidResponse Parse(string response)
    {
        var akbankVoidResponse = JObject.Parse(response);

        TxnCode = akbankVoidResponse["txnCode"]?.ToString();
        ResponseCode = akbankVoidResponse["responseCode"]?.ToString();
        ResponseMessage = akbankVoidResponse["responseMessage"]?.ToString();
        HostResponseCode = akbankVoidResponse["hostResponseCode"]?.ToString();
        HostMessage = akbankVoidResponse["hostMessage"]?.ToString();
        TxnDateTime = DateTime.Parse(akbankVoidResponse["txnDateTime"]?.ToString());
        OrderId = akbankVoidResponse["orderId"]?.ToString();

        return this;
    }
}
