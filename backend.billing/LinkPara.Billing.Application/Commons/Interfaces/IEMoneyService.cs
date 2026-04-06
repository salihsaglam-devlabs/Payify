using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IEmoneyService
{
    Task<ProvisionPreviewResponse> PreviewProvisionAsync(ProvisionPreviewRequest request);
    Task<ProvisionResponse> CreateProvisionAsync(ProvisionRequest request);
    Task<ProvisionResponse> CancelProvisionAsync(string provisionReferenceId);
}