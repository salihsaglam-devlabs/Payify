using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Response;

public class IsbankPointInquiryResponse : IsbankResponseBase
{
    public decimal MaximilesTotalForMile { get; set; }
    public decimal MaxipointTotalForMile { get; set; }
    public decimal TotalMiles { get; set; }
    public decimal UsableAdvanceMiles { get; set; }
    public decimal MaximileDebtAmount { get; set; }
    public string MaximileCardType { get; set; }
    public decimal MaximileFactor { get; set; }
    
    public IsbankPointInquiryResponse Parse(string response)
    {
        var isbankNonSecureResponse = JObject.Parse(response);

        var data = isbankNonSecureResponse.GetValue("data");

        if (data is not null)
        {
            ResponseCode = data["response_code"]?.ToString();
            RetrievalReferenceNumber = (long)data["retrieval_reference_number"];
            AuthCode = data["auth_code"]?.ToString();
            OrderNumber = data["order_number"]?.ToString();
            AuthorizationDateTime = DateTime.Parse(data["authorization_date_time"]?.ToString());
            MaxipointTotalForMile = (decimal)data["maxipoint"];
        }
        
        var errorArray = isbankNonSecureResponse["errors"] as JArray;
        if (errorArray != null && errorArray.Count > 0)
        {
            var firstError = errorArray.First;
            Code = firstError["code"]?.ToString();
            Message = firstError["message"]?.ToString();
        }

        return this;
    }
}