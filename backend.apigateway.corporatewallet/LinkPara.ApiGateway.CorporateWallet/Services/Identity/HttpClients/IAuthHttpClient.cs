using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Identity.HttpClients;

public interface IAuthHttpClient
{
    Task<TokenDto> LoginAsync(LoginRequest request);
    Task LogoutAsync(LogoutRequest request);
    Task<UserRefreshTokenResponse> RefreshTokenAsync(UserRefreshTokenRequest request);
}
