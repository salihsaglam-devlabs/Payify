using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.PaycoreCustomer.Request;

public class CreateCustomerRequest
{
    public string WalletNumber { get; set; }
    public string ProductCode { get; set; }
    public string CustomerGroupCode { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public CustomerAddress[] CustomerAddresses { get; set; }
    public CustomerCommunication[] CustomerCommunications { get; set; }
    public CustomerLimit[] CustomerLimits { get; set; }
}