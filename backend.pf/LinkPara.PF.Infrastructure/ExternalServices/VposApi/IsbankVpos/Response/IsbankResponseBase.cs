using Newtonsoft.Json.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Response;

public class IsbankResponseBase
{
    public string ResponseCode { get; set; }
    public long RetrievalReferenceNumber { get; set; }
    public string AuthCode { get; set; }
    public string OrderNumber { get; set; }
    public string CustomMessage { get; set; }
    public DateTime AuthorizationDateTime { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }
    
    public IsbankResponseBase Parse(string response)
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