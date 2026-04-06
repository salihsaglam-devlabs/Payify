using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.HttpProviders.Emoney;

public interface IProvisionService
{
    Task<ProvisionResponse> ProvisionAsync(ProvisionRequest request);
    Task<ProvisionResponse> CancelProvisionAsync(ProvisionCancelRequest request);
    Task<ProvisionResponse> ReturnProvisionAsync(ProvisionReturnRequest request);
    Task<ProvisionPreviewResponse> ProvisionPreviewAsync(ProvisionPreviewRequest request);
    Task<ProvisionCashbackResponse> ProvisionCashbackAsync(ProvisionCashbackRequest request);
    Task<ProvisionCashbackResponse> CancelProvisionCashbackAsync(CancelProvisionCashbackRequest request);
    Task<InquireProvisionResponse> InquireProvisionAsync(InquireProvisionRequest request);
}