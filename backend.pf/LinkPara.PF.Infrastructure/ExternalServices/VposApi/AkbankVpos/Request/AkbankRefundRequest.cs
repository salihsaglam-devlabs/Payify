using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;

public class AkbankRefundRequest : AkbankRequestBase
{
    public string Amount { get; set; }
    public string BuildRequest()
    {
        var akbankRefundRequest = new
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
            transaction = new
            {
                amount = Amount,
                currencyCode = CurrencyCode,
            },
            subMerchant = new
            {
                subMerchantId = SubMerchantId
            }
        };

        return JsonConvert.SerializeObject(akbankRefundRequest, Formatting.Indented); ;
    }
}
