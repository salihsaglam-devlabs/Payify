using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Request;

public class IsbankRefundRequest : IsbankPaymentRequestBase
{
    public string OriginalRetrievalReferenceNumber { get; set; }
    public string OriginalOrderNumber { get; set; }
    
    public string BuildRequest()
    {
        var request = new
        {
            order_number = OrderNumber,
            trace_number = TraceNumber,
            merchant_number = MerchantNumber,
            currency = Currency,
            amount = Amount,
            type = Type,
            original_retrieval_reference_number = OriginalRetrievalReferenceNumber
        };

        return JsonConvert.SerializeObject(request, Formatting.Indented);
    }
}