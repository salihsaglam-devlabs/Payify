namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.Response;

public class PaycoreBaseResponse
{
    public string version { get; set; }
    public string buildId { get; set; }
    public int statusCode { get; set; }
    public string message { get; set; }
    public string correlationId { get; set; }
    public PaycoreException exception { get; set; }
}

public class PaycoreException
{
    public int level { get; set; }
    public string code { get; set; }
    public string message { get; set; }
    public object details { get; set; }
    public object validationErrors { get; set; }
}
