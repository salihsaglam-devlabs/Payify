using LinkPara.HttpProviders.Emoney.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.HttpProviders.Emoney;

public interface IAccountService
{
    Task<AccountResponse> GetAccountDetailAsync(GetAccountDetailRequest request);
    Task PatchAccountAsync(Guid accountId, JsonPatchDocument<PatchAccountDto> patchAccountDto);
    Task PatchAccountUserAsync(Guid accountId, Guid accountUserId, JsonPatchDocument<PatchAccountUserDto> patchAccountUserDto);
    Task<List<AccountUserResponse>> GetAccountUserListAsync(Guid accountId);
    Task<ParentAccountResponse> GetParentAccountByUserIdAsync(Guid userId);
}
