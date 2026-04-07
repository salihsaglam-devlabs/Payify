using LinkPara.ApiGateway.Services.Card.Models.Shared;

namespace LinkPara.ApiGateway.Services.Card.Models.CustomerWalletCard.Response;

public class GetCustomerWalletCardsResponse : PaycoreResponse
{
    public List<CustomerWalletCardResponseItem> Cards { get; set; }
}

public class CustomerWalletCardResponseItem
{
    public string CardNumber { get; set; }
    public string ProductCode { get; set; }
    public string StatusCode { get; set; }
}