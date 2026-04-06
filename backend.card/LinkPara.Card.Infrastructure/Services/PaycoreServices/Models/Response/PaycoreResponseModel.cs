using System.Text.Json.Serialization;

namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.Response;

public class PaycoreResponseModel<T> : PaycoreBaseResponse
{
    [JsonPropertyName("result")]
    public T Result { get; set; }
    public bool IsSuccess
    {
        get
        {
            return exception is null || statusCode == 200;//todo const
        }
    }
}
