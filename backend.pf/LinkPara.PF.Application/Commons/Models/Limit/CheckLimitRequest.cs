using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Limit;

public class CheckLimitRequest
{
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string CardToken { get; set; }
    public string Currency { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public TransactionType TransactionType { get; set; }
    public int InstallmentCount { get; set; }
    public string ThreeDSessionId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
}
