using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;

public class GenerateAccessTokenResponse
{
    public string AccessToken { get; set; }
    public string OrderId { get; set; }
    public AuthenticationMethod AuthenticationMethod { get; set; }
    public Secure3dType Secure3dType { get; set; }
}