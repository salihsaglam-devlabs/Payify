using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using LinkPara.Emoney.Application.Features.Provisions;
using LinkPara.Emoney.Application.Features.Provisions.Commands.CancelProvisionCashback;
using LinkPara.Emoney.Application.Features.Provisions.Commands.Provision;
using LinkPara.Emoney.Application.Features.Provisions.Commands.ProvisionCashback;
using LinkPara.Emoney.Application.Features.Provisions.Commands.ReturnProvision;
using LinkPara.Emoney.Application.Features.Provisions.Queries.InquireProvision;
using LinkPara.Emoney.Application.Features.Provisions.Queries.ProvisionPreview;
using LinkPara.Emoney.Application.Features.Wallets.Commands.CancelProvision;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IProvisionService
{
    Task<ProvisionResponse> ProvisionAsync(ProvisionCommand request, CancellationToken cancellationToken);
    Task<ProvisionResponse> CancelProvisionAsync(CancelProvisionCommand request, CancellationToken cancellationToken);
    Task<ProvisionPreviewResponse> ProvisionPreviewAsync(ProvisionPreviewQuery request);
    Task<InquireProvisionResponse> InquireProvisionAsync(InquireProvisionQuery request);
    Task<ProvisionCashbackResponse> ProvisionCashbackAsync(ProvisionCashbackCommand request, CancellationToken cancellationToken);
    Task<ProvisionCashbackResponse> CancelProvisionCashbackAsync(CancelProvisionCashbackCommand request, CancellationToken cancellationToken);
    Task<ProvisionResponse> ReturnProvisionAsync(ReturnProvisionCommand request, CancellationToken cancellationToken);
    Task<ProvisionChargebackResponse> ProvisionChargebackAsync(ProvisionChargebackCommand request, CancellationToken cancellationToken);
}