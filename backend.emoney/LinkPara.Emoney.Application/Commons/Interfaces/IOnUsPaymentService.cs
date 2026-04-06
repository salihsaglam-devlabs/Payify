using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using LinkPara.Emoney.Application.Features.OnUsPayments;
using LinkPara.Emoney.Application.Features.OnUsPayments.Commands;
using LinkPara.Emoney.Application.Features.OnUsPayments.Queries;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IOnUsPaymentService
{
    Task<OnUsPaymentRequest> GetOnUsPaymentDetailsAsync(Guid onUsPaymentRequestId);
    Task<PaginatedList<OnUsPaymentRequest>> GetOnUsPaymentsAsync(GetOnUsPaymentQuery request);
    Task<ProvisionPreviewResponse> ApproveOnUsPaymentAsync(ApproveOnUsPaymentCommand request);
    Task<OnUsPaymentResponse> InitOnUsPaymentAsync(InitOnUsPaymentCommand request);
    Task<Unit> OnUsPaymentUpdateStatusAsync(OnUsPaymentUpdateStatusCommand request);

}
