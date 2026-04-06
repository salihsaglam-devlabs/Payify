namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class MoneyTransferResponse
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public Guid TransactionId { get; set; }
}