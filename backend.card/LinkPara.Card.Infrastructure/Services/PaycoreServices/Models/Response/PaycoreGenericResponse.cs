
using System.Text.Json.Serialization;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.Response;

public class PaycoreGenericResponse<T> : PaycoreBaseResponse
{
    [JsonPropertyName("result")]
    public List<T> Result { get; set; }
    public bool IsSuccess
    {
        get
        {
            return exception is null || statusCode == 200;//todo const
        }
    }
}

