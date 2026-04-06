using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public interface ICorporateWalletHttpClient
{
    Task ActivateCorporateWalletUserAsync(ActivateCorporateWalletUserRequest request);
    Task AddCorporateWalletUserAsync(AddCorporateWalletUserRequest request);
    Task DeactivateCorporateWalletUserAsync(DeactivateCorporateWalletUserRequest request);
    Task<CorporateAccountDto> GetCorporateWalletAccountAsync();
    Task<CorporateWalletUserDto> GetCorporateWalletUserAsync(Guid id);
    Task<PaginatedList<CorporateWalletUserDto>> GetCorporateWalletUsersAsync(GetCorporateWalletUsersRequest request);
    Task UpdateCorporateWalletUserAsync(UpdateCorporateWalletUserRequest request);
}
