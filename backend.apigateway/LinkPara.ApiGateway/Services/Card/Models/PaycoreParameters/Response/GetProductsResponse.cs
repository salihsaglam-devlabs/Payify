using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreParameters.Response;

public class GetProductsResponse : PaycoreResponse
{
    public PaycoreProduct[] Result { get; set; }
}