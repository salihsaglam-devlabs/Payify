using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Request;

public class IsbankPointInquiryRequest : IsbankPaymentRequestBase
{
    public string BuildRequest()
    {
        var request = new
        {
            order_number = OrderNumber,
            trace_number = TraceNumber,
            merchant_number = MerchantNumber,
            card_number = CardNumber,
            card_expire_month = CardExpireMonth,
            card_expire_year = CardExpireYear,
            cvv = Cvv,
            type = "maxipoint"
        };

        return JsonConvert.SerializeObject(request, Formatting.Indented);
    }
}