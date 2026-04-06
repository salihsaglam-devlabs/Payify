using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.Billing.Infrastructure.Services.EMoneyServices;

public class EMoneyService : IEmoneyService
{
    private readonly IProvisionService _provisionService;

    public EMoneyService(IProvisionService provisionService)
    {
        _provisionService = provisionService;
    }

    public async Task<ProvisionResponse> CancelProvisionAsync(string provisionReferenceId)
    {
        try
        {
            return await _provisionService.CancelProvisionAsync(new ProvisionCancelRequest { ConversationId = provisionReferenceId });
        }
        catch (Exception exception)
        {
            return new ProvisionResponse
            {
                IsSucceed = false,
                ReferenceNumber = provisionReferenceId,
                ErrorCode = ApiErrorCode.ErrorCancellingProvision,
                ErrorMessage = $"ErrorCancellingProvision: {exception.Message}"
            };
        }
    }

    public async Task<ProvisionResponse> CreateProvisionAsync(ProvisionRequest request)
    {
        return await _provisionService.ProvisionAsync(request);
    }

    public async Task<ProvisionPreviewResponse> PreviewProvisionAsync(ProvisionPreviewRequest request)
    {
        return await _provisionService.ProvisionPreviewAsync(request);
    }
}
