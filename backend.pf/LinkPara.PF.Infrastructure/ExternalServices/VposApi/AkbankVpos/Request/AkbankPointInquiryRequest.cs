using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;

public class AkbankPointInquiryRequest : AkbankRequestBase
{
    public string CardNumber { get; set; }
    public string Cvv2 { get; set; }
    public string ExpireDate { get; set; }
    public string BuildRequest()
    {
        var akbankPointInquiryRequest = new
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
            transaction = new
            {
                amount = "1.00",
                currencyCode = 949,
                motoInd = 1,
                installCount = 1
            },
            card = new {
                cardNumber = CardNumber,
                cvv2 = Cvv2,
                expireDate = ExpireDate
            }
        };

        return JsonConvert.SerializeObject(akbankPointInquiryRequest, Formatting.Indented); ;
    }
}
