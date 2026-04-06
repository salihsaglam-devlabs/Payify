using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Requests;

public class UpdateRuleRequest
{
    public Guid RuleId { get; set; }
    public List<CashbackTestUserDto> TestUsers { get; set; }
    public DateTime RuleStartDate { get; set; }
    public DateTime RuleEndDate { get; set; }
    public List<string> KycTypeList { get; set; }
}