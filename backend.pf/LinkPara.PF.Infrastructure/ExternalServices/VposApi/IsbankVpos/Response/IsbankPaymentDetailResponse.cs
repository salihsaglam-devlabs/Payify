using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Response;

public class IsbankPaymentDetailResponse
{
    public string ResponseDateTime { get; set; }
    public string TrnxDate { get; set; }
    public string TrnxTime { get; set; }
    public int TrnxType { get; set; }
    public string TrnxSubType { get; set; }
    public string CardNumber{ get; set; }
    public decimal Amount { get; set; }
    public int Installment { get; set; }
    public int Currency { get; set; }
    public string CurrencyDescription { get; set; }
    public string TrnxResponseCode { get; set; }
    public string TrnxResponseStatus { get; set; }
    public string MerchantNumber { get; set; }
    public string OriginalRetrievalReferenceNumber { get; set; }
    public string OrderNumber { get; set; }
    
    public IsbankPaymentDetailResponse Parse(string response)
    {
        var isbankNonSecureResponse = JObject.Parse(response);

        var data = isbankNonSecureResponse.GetValue("data");
        
        ResponseDateTime = isbankNonSecureResponse["response_date_time"]?.ToString();

        if (data is not null)
        {
            if (data["items"] is JArray items)
            {
                var item = items[^1];
                if (item is not null)
                {
                    OrderNumber = item["order_number"]?.ToString();
                    TrnxDate = item["trnx_date"]?.ToString();
                    TrnxTime = item["trnx_time"]?.ToString();
                    Amount = item["amount"]?.Value<decimal>() ?? 0;
                    TrnxResponseStatus = item["trnx_response_status"]?.ToString();
                    TrnxResponseCode = item["trnx_response_code"]?.ToString();
                    TrnxType = item["trnx_type"]?.Value<int>() ?? 0;
                    TrnxSubType = item["trnx_sub_type"]?.ToString();
                    CardNumber = item["card_number"]?.ToString();
                    Installment = item["installment"]?.Value<int>() ?? 0;
                    Currency = item["currency"]?.Value<int>() ?? 0;
                    CurrencyDescription = item["currency_description"]?.ToString();
                    MerchantNumber = item["merchant_number"]?.ToString();
                    OriginalRetrievalReferenceNumber = item["original_retrieval_reference_number"]?.ToString();
                }
            }
        }
        return this;
    }
}