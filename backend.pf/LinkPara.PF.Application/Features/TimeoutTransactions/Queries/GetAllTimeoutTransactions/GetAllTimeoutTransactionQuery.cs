using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.TimeoutTransactions.Queries.GetAllTimeoutTransactions;

public class GetAllTimeoutTransactionQuery : SearchQueryParams, IRequest<PaginatedList<TimeoutTransactionDto>>
{
    public Guid? MerchantId { get; set; }
    public int? AcquireBankCode { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
    public TransactionType? TransactionType { get; set; }
    public string OriginalOrderId { get; set; }
    public string ConversationId { get; set; }
    public TimeoutTransactionStatus? TimeoutTransactionStatus { get; set; }
    public string CardFirstNumbers { get; set; }
    public string CardLastNumbers { get; set; }
}

public class GetAllTimeoutTransactionQueryHandler : IRequestHandler<GetAllTimeoutTransactionQuery, PaginatedList<TimeoutTransactionDto>>
{
    private readonly IGenericRepository<TimeoutTransaction> _timeoutRepository;
    private readonly IMapper _mapper;

    public GetAllTimeoutTransactionQueryHandler(IGenericRepository<TimeoutTransaction> timeoutRepository,
        IMapper mapper)
    {
        _timeoutRepository = timeoutRepository;
        _mapper = mapper;
    }
    public async Task<PaginatedList<TimeoutTransactionDto>> Handle(GetAllTimeoutTransactionQuery request, CancellationToken cancellationToken)
    {
        var timeoutTransactions = _timeoutRepository.GetAll()
            .Include(b => b.Merchant).Include(b=>b.AcquireBank).AsQueryable();

        if (!string.IsNullOrEmpty(request.CardFirstNumbers))
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.CardNumber.Substring(0, 6).Contains(request.CardFirstNumbers));
        }

        if (!string.IsNullOrEmpty(request.CardLastNumbers))
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.CardNumber.Substring(12, 4).Contains(request.CardLastNumbers));
        }

        if (!string.IsNullOrEmpty(request.ConversationId))
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.ConversationId.Contains(request.ConversationId));
        }

        if (!string.IsNullOrEmpty(request.OriginalOrderId))
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.OriginalOrderId.Contains(request.OriginalOrderId));
        }

        if (request.AcquireBankCode is not null)
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.AcquireBankCode
                               == request.AcquireBankCode);
        }

        if (request.MerchantId is not null)
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.MerchantId
                               == request.MerchantId);
        }

        if (request.TransactionDateStart is not null)
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.TransactionDate
                               >= request.TransactionDateStart);
        }

        if (request.TransactionDateEnd is not null)
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.TransactionDate
                               <= request.TransactionDateEnd);
        }

        if (request.TimeoutTransactionStatus is not null)
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.TimeoutTransactionStatus
                               == request.TimeoutTransactionStatus);
        }

        if (request.TransactionType is not null)
        {
            timeoutTransactions = timeoutTransactions.Where(b => b.TransactionType
                               == request.TransactionType);
        }

        var list = await timeoutTransactions
            .PaginatedListWithMappingAsync<TimeoutTransaction,TimeoutTransactionDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

        return list;
    }
}
