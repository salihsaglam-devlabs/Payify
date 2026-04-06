namespace LinkPara.HttpProviders.Identity.Models;

public class GetIdentityParametersRequest
{
    public string Language { get; set; }
    public Guid UserId { get; set; }
}