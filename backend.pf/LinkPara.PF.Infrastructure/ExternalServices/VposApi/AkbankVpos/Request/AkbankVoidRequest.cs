using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;

public class AkbankVoidRequest : AkbankRequestBase
{
    public string BuildRequest()
    {
        var akbankVoidRequest = new
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

        return JsonConvert.SerializeObject(akbankVoidRequest, Formatting.Indented);
    }
}
