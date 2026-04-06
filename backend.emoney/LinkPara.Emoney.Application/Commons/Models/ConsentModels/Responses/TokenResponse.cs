namespace LinkPara.Emoney.Application.Commons.Models.ConsentModels.Responses;

public class TokenResponse
{
    public string Access_Token { get; set; }
    public int Expires_In { get; set; }
    public string Token_Type { get; set; }
    public string Scope { get; set; }
}
