namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Request;

public class UpdateCustomerRequest
{
    public string WalletNumber { get; set; }
    public string CustomerNumber { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
}