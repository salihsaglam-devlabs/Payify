using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;

public class CashbackRuleSummaryDto
{
    public Guid Id { get; set; }
    public CashbackProcessType ProcessType { get; set; }
    public DateTime RuleStartDate { get; set; }
    public DateTime RuleEndDate { get; set; }
    public CalculationMethod CalculationMethod { get; set; }
    public decimal Percentage { get; set; }
    public decimal FixedAmount { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public List<CashbackTestUserDto> TestUsers { get; set; }
    public DateTime CreateDate { get; set; }
}
