using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;

public class CashbackRuleDto
{
    public Guid Id { get; set; }
    public CashbackProcessType ProcessType { get; set; }
    public List<string>? KycTypeList { get; set; }
    public string? MccCode { get; set; }
    public string? CorporateWalletNumber { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public DateTime RuleStartDate { get; set; }
    public DateTime RuleEndDate { get; set; }
    public CalculationMethod CalculationMethod { get; set; }
    public decimal Percentage { get; set; }
    public decimal FixedAmount { get; set; }
    public decimal MaxEarningAmount { get; set; }
    public string Description { get; set; }
    public string NotificationContent { get; set; }
    public bool? IsTestCashback { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public List<CashbackTestUserDto>? TestUsers { get; set; }

}
