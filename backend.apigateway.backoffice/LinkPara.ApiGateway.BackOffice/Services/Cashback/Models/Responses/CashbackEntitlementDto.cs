using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;

public class CashbackEntitlementDto
{
    public Guid Id { get; set; }
    public Guid RuleId { get; set; }
    public string WalletNumber { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public decimal TransactionAmount { get; set; }
    public decimal CashbackAmount { get; set; }
    public string CurrencyCode { get; set; }
    public CashbackProcessType ProcessType { get; set; }
    public CashbackPaymentStatus PaymentStatus { get; set; }
    public CashbackProcessStatus ProcessStatus { get; set; }
    public string AccountKycLevel { get; set; }
    public DateTime PaymentDate { get; set; }
    public DateTime EntitlementDate { get; set; }
    public string FailedReason { get; set; }
    public bool IsMaxEarning { get; set; }
    public string ConversationId { get; set; }
    public string CorporateWalletNumber { get; set; }
    public string CorporateAccountName { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public RecordStatus RuleStatus { get; set; }
}
