using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Request;

public class UpdateCustomerCommunicationRequest
{
    public string WalletNumber { get; set; }
    public string CustomerNumber { get; set; }
    public CustomerCommunication[] CustomerCommunications { get; set; }
}