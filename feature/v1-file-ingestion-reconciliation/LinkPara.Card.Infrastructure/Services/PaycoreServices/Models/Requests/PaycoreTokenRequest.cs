namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.Requests;

public class PaycoreTokenRequest
{
    public int mbrId { get; set; }
    public string userCode { get; set; }
    public string password { get; set; }
    public int sessionTimeout { get; set; }
}
