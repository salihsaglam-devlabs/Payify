using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Requests;

public class CreateRuleRequest
{
    public CashbackProcessType ProcessType { get; set; }
    public List<string> KycTypeList { get; set; }
    public string MccCode { get; set; }
    public string CorporateWalletNumber { get; set; }
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
    public List<CashbackTestUserDto> TestUsers { get; set; }

}