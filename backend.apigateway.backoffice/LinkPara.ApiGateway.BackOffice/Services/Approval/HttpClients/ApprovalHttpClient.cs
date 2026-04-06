using LinkPara.ApiGateway.BackOffice.Exceptions;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;
using LinkPara.ApiGateway.BackOffice.Services.Approval.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.BackOffice.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;
using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.Approval.Models;
using LinkPara.HttpProviders.Utility;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.UrlModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.HttpClients;

public class ApprovalHttpClient : HttpClientBase, IApprovalHttpClient
{
    private readonly IUserHttpClient _userHttpClient;
    private readonly IBackOfficeHttpClient _backOfficeHttpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<BackOfficeHttpClient> _logger;
    private readonly IStringLocalizer _localizer;


    public ApprovalHttpClient(HttpClient client,
          IHttpContextAccessor httpContextAccessor,
          IUserHttpClient userHttpClient,
          IBackOfficeHttpClient backOfficeHttpClient,
          IVaultClient vaultClient,
          ILogger<BackOfficeHttpClient> logger,
        IStringLocalizerFactory factory) : base(client, httpContextAccessor)
    {
        _userHttpClient = userHttpClient;
        _backOfficeHttpClient = backOfficeHttpClient;
        _httpContextAccessor = httpContextAccessor;
        _vaultClient = vaultClient;
        _logger = logger;
        _localizer = factory.Create("Exceptions", "LinkPara.ApiGateway.BackOffice");
    }

    public async Task<PaginatedList<CaseDto>> GetActiveCasesAsync()
    {
        var request = new GetFilterCaseRequest
        {
            RecordStatus = RecordStatus.Active,
            Size = Int32.MaxValue
        };

        var url = CreateUrlWithParams($"v1/Cases", request, true);

        var response = await GetAsync(url);
        var cases = await response.Content.ReadFromJsonAsync<PaginatedList<CaseDto>>();
        return cases ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<CaseDto>> GetAllCasesAsync(GetFilterCaseRequest request)
    {
        var url = CreateUrlWithParams($"v1/Cases", request, true);
        var response = await GetAsync(url);
        var cases = await response.Content.ReadFromJsonAsync<PaginatedList<CaseDto>>();
        return cases ?? throw new InvalidOperationException();
    }

    public async Task<ApprovalResponse> ApproveAsync(BaseApproveRequest request)
    {
        var checkerRoles = await _userHttpClient.GetUserRolesAsync(request.UserId);
        var user = await _userHttpClient.GetUserByIdAsync(request.UserId);

        if (!checkerRoles.Any())
        {
            throw new NotFoundException(nameof(RoleDto), request.UserId);
        }

        var approveRequest = new ApproveRequest
        {
            UserId = request.UserId,
            CheckerRoleIds = checkerRoles.Select(s => s.Id).ToList(),
            RequestId = request.RequestId,
            CheckerFullName = user.FullName,
            Description = request.Description
        };

        var result = await PostAsJsonAsync("v1/Requests/approve", approveRequest);
        var approveResponse = await result.Content.ReadFromJsonAsync<ApprovalResponse>();

        if (approveResponse.Status == ApprovalStatus.Approved)
        {
            try
            {
                var res = await _backOfficeHttpClient.RecallApprovedRequestAsync(approveResponse.Request);
                approveResponse.HttpResponseContent = res;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ThrowCallingBackOffice requestId : {approveResponse.RequestId}, Error : {ex}");

                var patchRequestDto = new JsonPatchDocument<PatchRequestDto>();

                patchRequestDto.Replace(x => x.Status, ApprovalStatus.Error);
                patchRequestDto.Replace(x => x.ExceptionMessage, ex.Message);
                patchRequestDto.Replace(x => x.ExceptionDetails, ex.StackTrace);

                await PatchRequestAsync(approveResponse.RequestId, patchRequestDto);

                var message = _localizer.GetString("ApprovedRequestException").Value;

                message = string.Format(message, approveResponse.Request.DisplayName, ex.Message);

                throw new ApprovedRequestException("205", message);
            }
        }

        return approveResponse;
    }

    private async Task PatchRequestAsync(Guid requestId, JsonPatchDocument<PatchRequestDto> patchRequestDto)
    {
        _ = await PatchAsync($"v1/Requests/{requestId}", patchRequestDto);
    }

    public async Task RejectAsync(BaseRejectRequest request)
    {
        var checkerRoles = await _userHttpClient.GetUserRolesAsync(request.UserId);
        var user = await _userHttpClient.GetUserByIdAsync(request.UserId);

        if (!checkerRoles.Any())
        {
            throw new NotFoundException(nameof(RoleDto), request.UserId);
        }

        var rejectRequest = new RejectRequest
        {
            UserId = request.UserId,
            CheckerRoleIds = checkerRoles.Select(s => s.Id).ToList(),
            RequestId = request.RequestId,
            CheckerFullName = user.FullName,
            Reason = request.Reason
        };

        await PostAsJsonAsync("v1/Requests/reject", rejectRequest);
    }

    public async Task<ApprovalResponse> SaveRequestAsync(ApprovalRequest request)
    {
        var result = await PostAsJsonAsync("v1/Requests", request);
        var approveResponse = await result.Content.ReadFromJsonAsync<ApprovalResponse>();
        return approveResponse ?? throw new InvalidOperationException();
    }

    public async Task<ApprovalResponse> DuplicateRequestAsync(DuplicateRequest request)
    {
        var result = await PostAsJsonAsync("v1/Requests/duplicate", request);
        var approveResponse = await result.Content.ReadFromJsonAsync<ApprovalResponse>();
        return approveResponse ?? throw new InvalidOperationException();
    }

    public async Task<RequestScreenFields> GetRequestWithScreenFieldsAsync(Guid id)
    {
        var response = await GetAsync($"v1/Requests/{id}");
        var approvalRequest = await response.Content.ReadFromJsonAsync<RequestDto>();
        var screenFieldClient = GetScreenFieldsHttpClient(approvalRequest.ModuleName);

        var language = _httpContextAccessor.HttpContext.Request.Headers["Accept-Language"];
        screenFieldClient.DefaultRequestHeaders.Add("Accept-Language", (string)language);

        var authorization = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
        screenFieldClient.DefaultRequestHeaders.Add("Authorization", (string)authorization);

        var displayscreenFieldsResult = await screenFieldClient.GetAsync($"v1/Approvals?Resource={approvalRequest.Resource}" +
                                                                         $"&Action={approvalRequest.ActionType}" +
                                                                         $"&Body={approvalRequest.Body}" +
                                                                         $"&Url={approvalRequest.Url}" +
                                                                         $"&QueryParameters={approvalRequest.QueryParameters}");

        if (!displayscreenFieldsResult.IsSuccessStatusCode)
        {
            return new RequestScreenFields
            {
                Request = approvalRequest,
                DisplayScreenFields = new ApprovalScreenResponse()
            };
        }

        var displayScreenFieldsJson = await displayscreenFieldsResult.Content.ReadAsStringAsync();

        var displayScreenFields = JsonConvert.DeserializeObject<ApprovalScreenResponse>(displayScreenFieldsJson);

        return new RequestScreenFields
        {
            Request = approvalRequest,
            DisplayScreenFields = displayScreenFields
        };
    }

    public async Task<ApprovalRequestSummaryDto> GetRequestByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Requests/{id}");

        return await response.Content.ReadFromJsonAsync<ApprovalRequestSummaryDto>()
               ?? throw new InvalidOperationException();
    }

    private HttpClient GetScreenFieldsHttpClient(string moduleName)
    {
        var httpClient = new HttpClient();
        var serviceUrls = _vaultClient.GetSecretValue<ServiceUrlContainer>("SharedSecrets", "ServiceUrls");
        switch (moduleName)
        {
            case "PF":
                httpClient.BaseAddress = new Uri(serviceUrls.Pf);
                break;
            case "Identity":
                httpClient.BaseAddress = new Uri(serviceUrls.Identity);
                break;
            case "Emoney":
                httpClient.BaseAddress = new Uri(serviceUrls.Emoney);
                break;
            case "BusinessParameter":
                httpClient.BaseAddress = new Uri(serviceUrls.BusinessParameter);
                break;
            case "Billing":
                httpClient.BaseAddress = new Uri(serviceUrls.Billing);
                break;
            case "Approval":
                httpClient.BaseAddress = new Uri(serviceUrls.Approval);
                break;
            case "CustomerManagement":
                httpClient.BaseAddress = new Uri(serviceUrls.CustomerManagement);
                break;
            case "MoneyTransfer":
                httpClient.BaseAddress = new Uri(serviceUrls.MoneyTransfer);
                break;
            case "Accounting":
                httpClient.BaseAddress = new Uri(serviceUrls.Accounting);
                break;
            case "Cashback":
                httpClient.BaseAddress = new Uri(serviceUrls.Cashback);
                break;
            default:
                throw new InvalidOperationException("UndefinedModule");

        }
        return httpClient;
    }

    public async Task<RequestDto> GetRequestAsync(Guid id)
    {
        var response = await GetAsync($"v1/Requests/{id}");
        var approvalRequest = await response.Content.ReadFromJsonAsync<RequestDto>();
        return approvalRequest ?? throw new InvalidOperationException();
    }

    public async Task UpdateCaseAsync(UpdateCaseRequest request)
    {
        await PutAsJsonAsync("v1/Cases", request);
    }

    public async Task SaveMakerCheckerAsync(SaveMakerCheckerRequest request)
    {
        await PostAsJsonAsync("v1/MakerCheckers", request);
    }

    public async Task UpdateMakerCheckerAsync(UpdateMakerCheckerRequest request)
    {
        await PutAsJsonAsync("v1/MakerCheckers", request);
    }

    public async Task DeleteMakerCheckerAsync(Guid id)
    {
        await DeleteAsync($"v1/MakerCheckers/{id}");
    }

    public async Task<CaseDto> GetCaseByIdAsync(Guid id)
    {
        var response = await GetAsync($"v1/Cases/{id}");
        var approvalCase = await response.Content.ReadFromJsonAsync<CaseDto>();
        return approvalCase ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<RequestDto>> GetAllAuthorizedRequests(GetFilterApprovalRequest request)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            throw new NotFoundException("httpContextIsNotFound");
        }

        var contextUserId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(contextUserId) || !Guid.TryParse(contextUserId, out Guid userId))
        {
            throw new NotFoundException("UserIdIsNotFound");
        }

        var userRoles = await _userHttpClient.GetUserRolesAsync(userId);

        request.UserRoleIds = userRoles.Select(s => s.Id).ToArray();

        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Requests", request, true);

        var response = await GetAsync(url);
        var approvalRequests = await response.Content.ReadFromJsonAsync<PaginatedList<RequestDto>>();
        return approvalRequests ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<RequestCashbackDto>> GetAllCashbackRequests(GetFilterCashbackApprovalRequest request)
    {
        var url = GetQueryString.CreateUrlWithSearchQueryParams($"v1/Requests/cashback", request, true);

        var response = await GetAsync(url);
        var approvalRequests = await response.Content.ReadFromJsonAsync<PaginatedList<RequestCashbackDto>>();
        if (approvalRequests == null || approvalRequests.Items == null)
        {
            throw new InvalidOperationException();
        }

        return approvalRequests;
    }
}