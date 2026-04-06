using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GenerateAccessTokenRequest
{
    public string AccountKey { get; set; }
    public bool IsMsisdnValidated { get; set; }
    public string AuthenticationMethod { get; set; }
    public string Secure3dType { get; set; }
    public string CurrencyCode { get; set; }
    public string OrderNo { get; set; }
}

public class GenerateAccessTokenServiceRequest : GenerateAccessTokenRequest, IHasUserId
{
    public Guid UserId { get; set; }
}