using LinkPara.ApiGateway.BackOffice.Services.Approval.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Epin;

public class CashbackController : ApiControllerBase
{
    private readonly ICashbackHttpClient _cashbackHttpClient;
    private readonly IApprovalHttpClient _approvalHttpClient;


    public CashbackController(ICashbackHttpClient cashbackHttpClient, IApprovalHttpClient approvalHttpClient)
    {
        _cashbackHttpClient = cashbackHttpClient;
        _approvalHttpClient = approvalHttpClient;
    }

    /// <summary>
    /// This is a method used to get filtered rule list.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CashbackRule:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<CashbackRuleSummaryDto>>> GetFilteredRulesAsync([FromQuery] GetFilteredRulesRequest request)
    {
        return await _cashbackHttpClient.GetFilteredRulesAsync(request);
    }

    /// <summary>
    /// This is a method used to get rule detail info.
    /// </summary>
    /// <param name="ruleId"></param>
    /// <returns></returns>
    [Authorize(Policy = "CashbackRule:Read")]
    [HttpGet("{ruleId}")]
    public async Task<ActionResult<CashbackRuleDto>> GetByIdAsync([FromRoute] Guid ruleId)
    {
        return await _cashbackHttpClient.GetByIdAsync(ruleId);
    }

    /// <summary>
    /// This is a method used to create a new rule
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CashbackRule:Create")]
    [HttpPost("")]
    public async Task<CreateRuleResponse> CreateRuleAsync([FromBody] CreateRuleRequest request)
    {
        return await _cashbackHttpClient.CreateRuleAsync(request);
    }

    /// <summary>
    /// This is a method used to update a rule
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("")]
    [Authorize(Policy = "CashbackRule:Update")]
    public async Task UpdateRuleAsync([FromBody] UpdateRuleRequest request)
    {
        await _cashbackHttpClient.UpdateRuleAsync(request);
    }

    /// <summary>
    /// Set record's status to passive
    /// </summary>
    /// <param name="ruleId"></param>
    /// <returns></returns>
    [Authorize(Policy = "CashbackRule:Delete")]
    [HttpDelete("{ruleId}")]
    public async Task DeleteRuleAsync([FromRoute] Guid ruleId)
    {
        await _cashbackHttpClient.DeleteRuleAsync(ruleId);
    }

    /// <summary>
    /// This is a method used to validate a rule
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CashbackRule:Create")]
    [HttpPost("validate-rule")]
    public async Task<ValidateRuleResponse> ValidateRuleAsync([FromBody] ValidateRuleRequest request)
    {
        return await _cashbackHttpClient.ValidateRuleAsync(request);
    }


    /// <summary>
    /// This is a method used to get filtered cashback transactions
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "CashbackTransaction:ReadAll")]
    [HttpGet("cashback-transaction")]
    public async Task<ActionResult<PaginatedList<CashbackEntitlementDto>>> GetFilteredTransactionAsync([FromQuery] GetCashbackTransactionRequest request)
    {
        return await _cashbackHttpClient.GetFilteredTransactionAsync(request);
    }

    /// <summary>
    /// Get all cashback requests.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("approval-requests")]
    [Authorize(Policy = "CashbackRule:ReadAll")]
    public async Task<PaginatedList<RequestCashbackDto>> GetAllCashbackRequests([FromQuery] GetFilterCashbackApprovalRequest request)
    {
        return await _approvalHttpClient.GetAllCashbackRequests(request);
    }

}
