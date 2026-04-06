using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public interface ICorporateWalletHttpClient
{
    Task ActivateCorporateWalletAccountAsync(Guid id);
    Task ActivateCorporateWalletUserAsync(ActivateCorporateWalletUserRequest request);
    Task AddCorporateWalletUserAsync(AddCorporateWalletUserRequest request);
    Task DeactivateCorporateWalletAccountAsync(Guid id);
    Task DeactivateCorporateWalletUserAsync(DeactivateCorporateWalletUserRequest request);
    Task<CorporateAccountDto> GetCorporateWalletAccountAsync(Guid id);
    Task<PaginatedList<CorporateAccountDto>> GetCorporateWalletAccountsAsync(GetCorporateAccountsRequest request);
    Task<CorporateWalletUserDto> GetCorporateWalletUserAsync(Guid id);
    Task<PaginatedList<CorporateWalletUserDto>> GetCorporateWalletUsersAsync(GetCorporateWalletUsersRequest request);
    Task UpdateCorporateWalletAccountAsync(UpdateCorporateAccountRequest request);
    Task UpdateCorporateWalletUserAsync(UpdateCorporateWalletUserRequest request);
}
