using LinkPara.Accounting.Application.Features.Payments;
using LinkPara.Accounting.Application.Features.Payments.Commands.CancelPayment;
using LinkPara.Accounting.Application.Features.Payments.Commands.DeletePayment;
using LinkPara.Accounting.Application.Features.Payments.Commands.PostPayment;
using LinkPara.Accounting.Application.Features.Payments.Queries.GetFilterPayment;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Accounting.Application.Commons.Interfaces;

public interface IPaymentService
{
    Task CancelPaymentAsync(CancelPaymentCommand request);
    Task<Unit> DeletePaymentAsync(DeletePaymentCommand request);
    Task<PaymentDto> GetByIdAsync(Guid id);
    Task<PaginatedList<PaymentDto>> GetFilterPaymentAsync(GetFilterPaymentQuery request);
    Task PostPaymentAsync(PostPaymentCommand request);
}
