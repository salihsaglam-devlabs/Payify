using System.Text.Encodings.Web;
using System.Text.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Request;

public class OzanPayPostAuthRequest : OzanPaymentBase
{
    public string BuildRequest()
    {
        var requestObject = new
        {
            apiKey = ApiKey,
            amount = Amount,
            currency = Currency,
            referenceNo = ReferenceNo,
            transactionId = TransactionId,
        };

        return JsonSerializer.Serialize(requestObject, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}
