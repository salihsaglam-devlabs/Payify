using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IEmoneyService
{
    Task<ProvisionResponse> CreateProvisionAsync(ProvisionRequest request);
    Task<ProvisionResponse> CancelProvisionAsync(string provisionReferenceId);
    Task<ProvisionPreviewResponse> PreviewProvisionAsync(ProvisionPreviewRequest request); 
    Task<ProvisionCashbackResponse> ProvisionCashbackAsync(ProvisionCashbackRequest request);
    Task<ProvisionCashbackResponse> CancelProvisionCashbackAsync(CancelProvisionCashbackRequest request);
    Task<ProvisionResponse> ReturnProvisionAsync(ProvisionReturnRequest request);
}