using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface IEmoneyAccountHttpClient
{
    Task<AccountDto> GetAccountByIdAsync(Guid id);
    Task<PaginatedList<AccountUserDto>> GetAllAccountUserAsync(GetAllAccountUserRequest request);
    Task<PaginatedList<AccountDto>> GetAccountListAsync(GetAccountListRequest request);
    Task<List<AccountUserDto>> GetAccountUserListAsync(Guid accountId);
    Task<List<WalletDto>> GetAccountWalletListAsync(Guid accountId);
    Task<AccountDto> PatchAsync(Guid accountId, JsonPatchDocument<UpdateAccountRequest> request);
    Task<List<AccountKycChangeDto>> GetAccountKycChangesByIdAsync(Guid id);
    Task<PaginatedList<CustodyAccountResponse>> GetCustodyAccountListAsync(GetCustodyAccountListRequest request);
    Task DeactivateAccountAsync(DeactivateAccountRequest request);
}
