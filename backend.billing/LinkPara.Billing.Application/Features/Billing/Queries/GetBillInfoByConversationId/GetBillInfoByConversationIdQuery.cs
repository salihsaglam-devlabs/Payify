using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Billing.Application.Features.Billing.Queries.GetBillInfoByConversationId;

public class GetBillInfoByConversationIdQuery : SearchQueryParams, IRequest<BillTransactionResponseDto>
{
    public string ConversationId { get; set; }
}

public class GetBillInfoByConversationIdQueryHandler : IRequestHandler<GetBillInfoByConversationIdQuery, BillTransactionResponseDto>
{
    private readonly ITransactionService _transactionService;

    public GetBillInfoByConversationIdQueryHandler(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    public async Task<BillTransactionResponseDto> Handle(GetBillInfoByConversationIdQuery request, CancellationToken cancellationToken)
    {
        return await _transactionService.GetBillInfoByConversationIdAsync(request);
    }
}