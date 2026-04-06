using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Services.Cashback.HttpClients;

public interface ICashbackHttpClient
{
    Task<PaginatedList<CashbackRuleSummaryDto>> GetFilteredRulesAsync(GetFilteredRulesRequest request);
    Task<CashbackRuleDto> GetByIdAsync(Guid ruleId);
    Task<CreateRuleResponse> CreateRuleAsync(CreateRuleRequest request);
    Task UpdateRuleAsync(UpdateRuleRequest request);
    Task DeleteRuleAsync(Guid ruleId);
    Task<ValidateRuleResponse> ValidateRuleAsync(ValidateRuleRequest request);
    Task<PaginatedList<CashbackEntitlementDto>> GetFilteredTransactionAsync(GetCashbackTransactionRequest request);

}
