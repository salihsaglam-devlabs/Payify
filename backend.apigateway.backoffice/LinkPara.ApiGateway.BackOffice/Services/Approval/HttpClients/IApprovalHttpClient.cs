using LinkPara.ApiGateway.BackOffice.Services.Approval.Models;
using LinkPara.ApiGateway.BackOffice.Services.Cashback.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Approval.HttpClients;

public interface IApprovalHttpClient
{
    Task<PaginatedList<CaseDto>> GetActiveCasesAsync();
    Task<PaginatedList<CaseDto>> GetAllCasesAsync(GetFilterCaseRequest request);
    Task<ApprovalResponse> SaveRequestAsync(ApprovalRequest request);
    Task<ApprovalResponse> DuplicateRequestAsync(DuplicateRequest request);
    Task<ApprovalResponse> ApproveAsync(BaseApproveRequest request);
    Task RejectAsync(BaseRejectRequest request);
    Task UpdateCaseAsync(UpdateCaseRequest request);
    Task SaveMakerCheckerAsync(SaveMakerCheckerRequest request);
    Task UpdateMakerCheckerAsync(UpdateMakerCheckerRequest request);
    Task DeleteMakerCheckerAsync(Guid id);
    Task<CaseDto> GetCaseByIdAsync(Guid id);
    Task<PaginatedList<RequestDto>> GetAllAuthorizedRequests(GetFilterApprovalRequest request);
    Task<RequestScreenFields> GetRequestWithScreenFieldsAsync(Guid id);
    Task<ApprovalRequestSummaryDto> GetRequestByIdAsync(Guid id);
    Task<PaginatedList<RequestCashbackDto>> GetAllCashbackRequests(GetFilterCashbackApprovalRequest request);
}