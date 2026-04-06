
using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Accounting.Application.Features.Payments.Queries.GetFilterPayment;

public class GetFilterPaymentQuery : SearchQueryParams, IRequest<PaginatedList<PaymentDto>>
{
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public bool? IsSuccess { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public OperationType? OperationType { get; set; }
}

public class GetFilterPaymentQueryHandler : IRequestHandler<GetFilterPaymentQuery, PaginatedList<PaymentDto>>
{

    private readonly IPaymentService _paymentService;

    public GetFilterPaymentQueryHandler(
        IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<PaginatedList<PaymentDto>> Handle(GetFilterPaymentQuery request, CancellationToken cancellationToken)
    {
        return await _paymentService.GetFilterPaymentAsync(request);
    }
}