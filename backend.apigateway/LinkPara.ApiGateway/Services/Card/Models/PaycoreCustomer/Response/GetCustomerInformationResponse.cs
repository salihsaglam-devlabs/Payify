using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Response;

public class GetCustomerInformationResponse : PaycoreResponse
{
    public string WalletNumber { get; set; }
    public string CustomerNumber { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
}