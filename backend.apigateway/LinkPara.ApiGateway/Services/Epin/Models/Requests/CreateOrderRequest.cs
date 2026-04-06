namespace LinkPara.ApiGateway.Services.Epin.Models.Requests;

public class CreateOrderRequest
{
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
    public string WalletNumber { get; set; }
    public string CurrencyCode { get; set; }
    public Guid PublisherId { get; set; }
    public Guid BrandId { get; set; }
}
