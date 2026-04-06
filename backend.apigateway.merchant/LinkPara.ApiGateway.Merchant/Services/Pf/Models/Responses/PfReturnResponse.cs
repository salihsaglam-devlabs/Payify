using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class PfReturnResponse : ResponseModel
{
    public string ReturnMessage { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public decimal ReturnAmount { get; set; }
}
