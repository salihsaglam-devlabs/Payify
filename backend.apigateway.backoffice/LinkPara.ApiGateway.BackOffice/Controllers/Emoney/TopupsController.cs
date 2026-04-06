using LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Emoney;

public class TopupsController : ApiControllerBase
{
    private readonly ITopupHttpClient _httpClient;
    private readonly IEmoneyPricingProfileHttpClient _pricingProfileHttpClient;

    public TopupsController(ITopupHttpClient httpClient,
        IEmoneyPricingProfileHttpClient pricingProfileHttpClient)
    {
        _httpClient = httpClient;
        _pricingProfileHttpClient = pricingProfileHttpClient;
    }

    /// <summary>
    /// Topup cancel
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("cancel")]
    public async Task<TopupCancelResponse> TopupCancelAsync([FromBody] TopupCancelRequest request)
    {
        return await _httpClient.TopupCancelAsync(request);
    }

    /// <summary>
    /// Returns all topups.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet("")]
    [Authorize(Policy = "Topup:ReadAll")]
    public async Task<ActionResult<PaginatedList<TopupResponse>>> GetAllAsync([FromQuery] GetTopupListRequest request)
    {
        return await _httpClient.GetListAsync(request);
    }

    /// <summary>
    /// Topup return to wallet
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Create")]
    [HttpPost("return-to-wallet")]
    public async Task TopupReturnToWalletAsync([FromBody] TopupReturnToWalletRequest request)
    {
        await _httpClient.TopupReturnToWalletAsync(request);
    }

    /// <summary>
    /// Topup update status
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "Topup:Update")]
    [HttpPost("update-status")]
    public async Task TopupUpdateStatusAsync([FromBody] TopupUpdateStatusRequest request)
    {
        await _httpClient.TopupUpdateStatusAsync(request);
    }

    /// <summary>
    /// Creates a new profile with items
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("save-pricing-profile")]
    [Authorize(Policy = "Topup:Create")]
    public async Task SaveAsync(TopupPricingProfileRequest request)
    {
        await _pricingProfileHttpClient.SaveAsync(new EmoneySavePricingProfileRequest
        {
            ActivationDateStart = request.ActivationDateStart,
            BankCode = request.BankCode,
            CurrencyCode = request.CurrencyCode,
            TransferType = TransferType.CreditCardTopup,
            ProfileItems = new List<PricingProfileItemModel>
            {
                new()
                {
                    CommissionRate = request.ProfileItem.CommissionRate,
                    Fee = request.ProfileItem.Fee,
                    MaxAmount = request.ProfileItem.MaxAmount,
                    MinAmount = request.ProfileItem.MinAmount,
                    WalletType = request.ProfileItem.WalletType
                }
            },
            CardType = request.CardType
        });
    }
}
