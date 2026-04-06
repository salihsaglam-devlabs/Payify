using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;

public class AkbankPaymentDetailRequest : AkbankRequestBase
{
    public string BuildRequest()
    {
        var akbankDetailRequest = new
        {
            version = "1.00",
            txnCode = TxnCode,
            requestDateTime = RequestDateTime,
            randomNumber = RandomNumber,
            terminal = new
            {
                merchantSafeId = MerchantSafeId,
                terminalSafeId = TerminalSafeId
            },
            order = new
            {
                orderId = OrderId
            },
            subMerchant = new
            {
                subMerchantId = SubMerchantId
            }
        };

        return JsonConvert.SerializeObject(akbankDetailRequest, Formatting.Indented); ;
    }
}
