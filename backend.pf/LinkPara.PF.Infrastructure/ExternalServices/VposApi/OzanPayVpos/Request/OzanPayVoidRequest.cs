using System.Text.Encodings.Web;
using System.Text.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Request;

public class OzanPayVoidRequest : OzanPayRequestBase
{
    public string Amount { get; set; }
    public string Currency { get; set; }
    public string BuildRequest()
    {
        var requestObject = new
        {
            apiKey = ApiKey,
            referenceNo = ReferenceNo,
            transactionId = TransactionId,
            amount = Amount,
            currency = Currency,
        };

        return JsonSerializer.Serialize(requestObject, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
    public string BuildPreauthRequest()
    {
        var requestObject = new
        {
            apiKey = ApiKey,
            referenceNo = ReferenceNo,
            transactionId = TransactionId
        };

        return JsonSerializer.Serialize(requestObject, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}
