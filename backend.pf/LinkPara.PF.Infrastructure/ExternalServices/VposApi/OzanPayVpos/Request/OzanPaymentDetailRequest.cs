using System.Text.Encodings.Web;
using System.Text.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Request;

public class OzanPaymentDetailRequest : OzanPayRequestBase
{
    public string BuildRequest()
    {
        var requestObject = new
        {
            apiKey = ApiKey,
            referenceNo = ReferenceNo
        };

        return JsonSerializer.Serialize(requestObject, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}
