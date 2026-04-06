using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Response;

public class AkbankPayment3DModelResponse : AkbankResponseBase
{
    public AkbankPayment3DModelResponse Parse(string response)
    {
        var akbank3DResponse = JObject.Parse(response);

        TxnCode = akbank3DResponse["txnCode"]?.ToString();
        ResponseCode = akbank3DResponse["responseCode"]?.ToString();
        ResponseMessage = akbank3DResponse["responseMessage"]?.ToString();
        HostResponseCode = akbank3DResponse["hostResponseCode"]?.ToString();
        HostMessage = akbank3DResponse["hostMessage"]?.ToString();
        TxnDateTime = DateTime.Parse(akbank3DResponse["txnDateTime"]?.ToString());
        OrderId = akbank3DResponse["order"]["orderId"]?.ToString();
        AuthCode = akbank3DResponse["transaction"]["authCode"]?.ToString();
        RrnNumber = akbank3DResponse["transaction"]["rrn"]?.ToString();

        return this;
    }
}
