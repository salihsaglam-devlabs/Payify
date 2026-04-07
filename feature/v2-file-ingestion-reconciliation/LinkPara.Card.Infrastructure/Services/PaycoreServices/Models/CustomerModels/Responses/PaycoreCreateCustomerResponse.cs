namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CustomerModels.Responses;
public class PaycoreCreateCustomerResponse
{
    public int statusCode { get; set; }
    public string message { get; set; }
    public string result { get; set; }
    public string correlationId { get; set; }
}