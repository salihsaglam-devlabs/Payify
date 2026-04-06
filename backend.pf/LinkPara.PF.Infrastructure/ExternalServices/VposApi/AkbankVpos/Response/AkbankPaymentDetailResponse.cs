using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Response;

public class AkbankPaymentDetailResponse : AkbankResponseBase
{
    public AkbankPaymentDetailResponse Parse(string response)
    {
        var akbankDetailResponse = JObject.Parse(response);

        var txnDetail = akbankDetailResponse.GetValue("txnDetailList");

        TxnCode = txnDetail["txnCode"]?.ToString();
        ResponseCode = txnDetail["responseCode"]?.ToString();
        ResponseMessage = txnDetail["responseMessage"]?.ToString();
        HostResponseCode = txnDetail["hostResponseCode"]?.ToString();
        HostMessage = txnDetail["hostMessage"]?.ToString();
        TxnDateTime = DateTime.Parse(txnDetail["txnDateTime"]?.ToString());
        OrderId = txnDetail["orderId"]?.ToString();       
        AuthCode = txnDetail["authCode"]?.ToString();
        TxnStatus = txnDetail["txnStatus"]?.ToString();
        Amount = txnDetail["amount"].ToString();
        InstallCount = (int?)txnDetail["installCount"] ?? 1;

        return this;
    }
}
