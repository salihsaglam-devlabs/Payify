using LinkPara.HttpProviders.IKS.Models.Response;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.PricingPreview;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Merchants.Command.ApproveMerchant;
using LinkPara.PF.Application.Features.Merchants.Command.DeleteMerchant;
using LinkPara.PF.Application.Features.Merchants.Command.MerchantPricingPreview;
using LinkPara.PF.Application.Features.Merchants.Command.PatchMerchant;
using LinkPara.PF.Application.Features.Merchants.Command.PatchMerchantApiKey;
using LinkPara.PF.Application.Features.Merchants.Command.PutMerchantPanel;
using LinkPara.PF.Application.Features.Merchants.Command.SaveAnnulment;
using LinkPara.PF.Application.Features.Merchants.Command.UpdateMerchant;
using LinkPara.PF.Application.Features.Merchants.Command.UpdateMerchantIKS;
using LinkPara.PF.Application.Features.Merchants.Queries.GenerateApiKeys;
using LinkPara.PF.Application.Features.Merchants.Queries.GetAllDecryptedApiKeys;
using LinkPara.PF.Application.Features.Merchants.Queries.GetApiKeys;
using LinkPara.PF.Application.Features.Merchants.Queries.GetApiKeysByMerchantNumber;
using LinkPara.PF.Application.Features.Merchants.Queries.GetAuthorizedPhoneNumber;
using LinkPara.PF.Application.Features.Merchants.Queries.GetFilterMerchant;
using LinkPara.PF.Application.Features.Merchants.Queries.GetMerchantById;
using LinkPara.PF.Application.Features.Merchants.Queries.GetMerchantSummaryById;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class MerchantsController : ApiControllerBase
{
    /// <summary>
    /// Returns filtered merchants
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:ReadAll")]
    [HttpGet("")]
    public async Task<ActionResult<PaginatedList<MerchantDto>>> GetFilterAsync([FromQuery] GetFilterMerchantQuery query)
    {
        return await Mediator.Send(query);
    }

    /// <summary>
    /// Returns a merchant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Read")]
    [HttpGet("{id}")]
    public async Task<ActionResult<MerchantDto>> GetByIdAsync([FromRoute] Guid id)
    {
        var merchant = await Mediator.Send(new GetMerchantByIdQuery { Id = id });
        if (!merchant.MerchantBankAccounts.Any())
        {
            merchant.MerchantBankAccounts.Add(new MerchantBankAccountDto
            {
                Id = Guid.Empty,
                MerchantId = id,
                CreatedBy = null,
                RecordStatus = RecordStatus.Active,
                BankCode = -1,
                Iban = string.Empty
            });
        }

        return merchant;
    }


    /// <summary>
    /// Returns a merchant for merchant panel
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Read")]
    [HttpGet("summary")]
    public async Task<ActionResult<MerchantSummaryDto>> GetMerchantSummaryByIdAsync()
    {
        return await Mediator.Send(new GetMerchantSummaryByIdQuery());
    }

    /// <summary>
    /// Updates merchant content
    /// </summary>
    /// <param name="id"></param>
    /// <param name="updateMerchant"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Update")]
    [HttpPut("update/panel/{id}")]
    public async Task PutMerchantContentAsync(Guid id, UpdateMerchantPanelDto updateMerchant)
    {
        await Mediator.Send(new PutMerchantPanelCommand {Id = id, MerchantPanel = updateMerchant});
    }

    /// <summary>
    /// Updates a merchant
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Update")]
    [HttpPut("")]
    public async Task<MerchantResponse> UpdateAsync(UpdateMerchantCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Delete merchant
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteMerchantCommand { Id = id });
    }

    /// <summary>
    /// Get public/private key pair of merchant
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Read")]
    [HttpGet("apiKeys")]
    public async Task<MerchantApiKeyDto> GetApiKeysAsync([FromQuery] GetApiKeysQuery query)
    {
        return await Mediator.Send(query);
    }
    /// <summary>
    /// Get public/private key pair of merchant
    /// </summary>
    /// <param name="merchantNumber"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Read")]
    [HttpGet("apiKeys/{merchantNumber}")]
    public async Task<MerchantApiKeyDto> GetApiKeysByMerchantIdAsync([FromRoute] string merchantNumber)
    {
        return await Mediator.Send(new GetApiKeysByMerchantNumberQuery() { MerchantNumber = merchantNumber });
    }
    /// <summary>
    /// Generate api keys
    /// </summary>
    /// <param name="merchantId"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Read")]
    [HttpGet("{merchantId}/generate-apiKeys")]
    public async Task<MerchantApiKeyDto> GenerateApiKeysAsync([FromRoute] Guid merchantId)
    {
        return await Mediator.Send(new GenerateApiKeysQuery { MerchantId = merchantId });
    }

    /// <summary>
    /// Performs the approval/rejection process for the merchant
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Update")]
    [HttpPut("approve")]
    public async Task Approve(ApproveMerchantCommand command)
    {
        await Mediator.Send(command);
    }

    /// <summary>
    /// Updates merchant with patch
    /// </summary>
    /// <param name="id"></param>
    /// <param name="merchant"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Update")]
    [HttpPatch("update/{id}")]
    public async Task<UpdateMerchantRequest> Patch(Guid id, [FromBody] JsonPatchDocument<UpdateMerchantRequest> merchant)
    {
        return await Mediator.Send(new PatchMerchantCommand { Id = id, Merchant = merchant });
    }

    /// <summary>
    ///  Updates merchant api keys with patch
    /// </summary>
    /// <param name="merchantId"></param>
    /// <param name="merchantApiKey"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Update")]
    [HttpPatch("{merchantId}/apiKeys")]
    public async Task<MerchantApiKeyPatch> Patch(Guid merchantId, [FromBody] JsonPatchDocument<MerchantApiKeyPatch> merchantApiKey)
    {
        return await Mediator.Send(new PatchMerchantApiKeyCommand { MerchantId = merchantId, MerchantApiKey = merchantApiKey });
    }

    [Authorize(Policy = "MerchantIksAnnulment:Create")]
    [HttpPost("{merchantId}/annulments")]
    public async Task<IKSResponse<IKSAnnulmentResponse>> SaveAnnulmentAsync(Guid merchantId, [FromBody] SaveAnnulmentCommand command)
    {
        return await Mediator.Send(new SaveAnnulmentCommand
        {
            Id = merchantId,
            AnnulmentCode = command.AnnulmentCode,
            AnnulmentCodeDescription = command.AnnulmentCodeDescription,
            AnnulmentDescription = command.AnnulmentDescription,
            IsCancelCode = command.IsCancelCode
        });
    }

    /// <summary>
    /// Updates a merchant status
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "MerchantIksAnnulment:Update")]
    [HttpPut("updateMerchantIKS")]
    public async Task UpdateMerchantIKSAsync(UpdateMerchantIKSCommand command)
    {
        await Mediator.Send(command);
    }
    
    /// <summary>
    /// Checks loss-making rates in merchant pricing setup
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Update")]
    [HttpPost("pricing-preview")]
    public async Task<PricingProfilePreviewResponse> PreviewPricingProfileUpdateAsync(MerchantPricingPreviewCommand command)
    {
        return await Mediator.Send(command);
    }

    /// <summary>
    /// Returns an authorized phone number
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Policy = "Merchant:Read")]
    [HttpGet("authorizedPersonPhoneNumber/{id}")]
    public async Task<ActionResult<string>> GetAuthorizedPersonPhoneNumberAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetAuthorizedPhoneNumberQuery { Id = id });
    }
    
    /// <summary>
    /// Returns decrypted api key list
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("all-api-keys")]
    public async Task<List<MerchantApiKeyDto>> GetAllDecryptedApiKeysAsync()
    {
        return await Mediator.Send(new GetAllDecryptedApiKeysQuery());
    }
}
