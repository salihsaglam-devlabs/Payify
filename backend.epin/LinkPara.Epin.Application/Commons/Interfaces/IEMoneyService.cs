using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.Epin.Application.Commons.Interfaces;

public interface IEmoneyService
{
    Task<ProvisionResponse> CreateProvisionAsync(ProvisionRequest request);
    Task<ProvisionResponse> CancelProvisionAsync(string provisionReferenceId);
    Task<ProvisionPreviewResponse> PreviewProvisionAsync(ProvisionPreviewRequest request);
}