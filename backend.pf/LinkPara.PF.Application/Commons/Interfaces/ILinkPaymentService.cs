using LinkPara.PF.Application.Features.LinkPayments;
using LinkPara.PF.Application.Features.LinkPayments.Commands.SaveLinkPayment;
using LinkPara.PF.Application.Features.LinkPayments.Queries.GetPaymentDetail;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ILinkPaymentService
{
    Task<LinkPaymentResponse> SaveLinkPaymentAsync(SaveLinkPaymentCommand command);
    Task<PaginatedList<LinkPaymentDetailResponse>> GetLinkPaymentDetails(GetPaymentDetailQuery request);
}
