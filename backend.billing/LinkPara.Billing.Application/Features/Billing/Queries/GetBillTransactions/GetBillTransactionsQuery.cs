using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Billing.Application.Features.Billing.Queries.GetBillTransactions;

public class GetBillTransactionsQuery : SearchQueryParams, IRequest<PaginatedList<BillTransactionResponseDto>>
{
    public Guid? VendorId { get; set; }
    public Guid? SectorId { get; set; }
    public Guid? InstitutionId { get; set; }
    public string BillNumber { get; set; }
    public string PayeeFullName { get; set; }
    public DateTime? BillDueStartDate { get; set; }
    public DateTime? BillDueEndDate { get; set; }
    public DateTime? PaymentStartDate { get; set; }
    public DateTime? PaymentEndDate { get; set; }
    public TransactionStatus? TransactionStatus { get; set; }
}

public class GetBillTransactionsQueryHandler : IRequestHandler<GetBillTransactionsQuery, PaginatedList<BillTransactionResponseDto>>
{
    private readonly ITransactionService _transactionService;

    public GetBillTransactionsQueryHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<PaginatedList<BillTransactionResponseDto>> Handle(GetBillTransactionsQuery request, CancellationToken cancellationToken)
    {
        return await _transactionService.GetListAsync(request);
    }
}