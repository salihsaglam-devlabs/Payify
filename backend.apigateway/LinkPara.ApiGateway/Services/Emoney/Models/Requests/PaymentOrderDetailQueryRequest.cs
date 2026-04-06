using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class PaymentOrderDetailQueryRequest
{
    public string HhsCode { get; set; }
    public string AppUserId { get; set; }
    public string ConsentId { get; set; }
}

