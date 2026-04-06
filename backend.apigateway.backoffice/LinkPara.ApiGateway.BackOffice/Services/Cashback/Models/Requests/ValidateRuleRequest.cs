using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Requests;

public class ValidateRuleRequest
{
    public ValidateReason ValidateReason { get; set; }
    public Guid? RuleId { get; set; }
    public List<CashbackTestUserDto> TestUsers { get; set; }
    public DateTime RuleStartDate { get; set; }
    public DateTime RuleEndDate { get; set; }
    public List<string> KycTypeList { get; set; }
    public CashbackProcessType CashbackProcessType { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public CalculationMethod CalculationMethod { get; set; }
    public decimal MaxEarningAmount { get; set; }
    public decimal FixedAmount { get; set; }

}