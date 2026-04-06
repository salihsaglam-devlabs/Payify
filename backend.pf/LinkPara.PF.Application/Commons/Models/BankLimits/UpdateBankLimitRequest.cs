using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.BankLimits;

public class UpdateBankLimitRequest
{
    public Guid AcquireBankId { get; set; }
    public BankLimitType BankLimitType { get; set; }
    public decimal Amount { get; set; }
}
