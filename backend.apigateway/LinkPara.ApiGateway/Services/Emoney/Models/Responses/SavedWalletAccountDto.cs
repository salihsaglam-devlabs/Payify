namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class SavedWalletAccountDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Tag { get; set; }
    public string ReceiverName { get; set; }
    public string WalletNumber { get; set; }
    public Guid WalletOwnerAccountId { get; set; }
}
