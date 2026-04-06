namespace LinkPara.Emoney.Application.Commons.Models;

public class SendCashbackQueueRequest
{
    public Guid TransactionId { get; set; }
    public string CorporateWalletNumber { get; set; }
    public string CorporateAccountName { get; set; }
    public string ConversationId { get; set; }
}
