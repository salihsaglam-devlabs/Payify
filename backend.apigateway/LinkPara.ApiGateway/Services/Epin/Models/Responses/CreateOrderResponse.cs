namespace LinkPara.ApiGateway.Services.Epin.Models.Responses;

public class CreateOrderResponse
{
    public string WalletNumber { get; set; }
    public decimal Amount { get; set; }
    public string Pin { get; set; }
    public string Image { get; set; }
    public Guid? EmoneyTransactionId { get; set; }
}
