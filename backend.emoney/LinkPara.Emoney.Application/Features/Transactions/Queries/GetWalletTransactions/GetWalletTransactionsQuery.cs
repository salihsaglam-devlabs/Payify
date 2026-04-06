using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetWalletTransactions;

public class GetWalletTransactionsQuery : SearchQueryParams, IRequest<PaginatedList<TransactionDto>>
{
    public Guid WalletId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30).Add(-DateTime.Now.TimeOfDay);
    public DateTime EndDate { get; set; } = DateTime.Now;
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public TransactionDirection? Direction { get; set; }
    public TransactionType?[] TransactionTypes { get; set; }
}

public class GetWalletTransactionsQueryHandler : IRequestHandler<GetWalletTransactionsQuery, PaginatedList<TransactionDto>>
{
    private readonly IGenericRepository<Transaction> _repository;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _tagLocalizer;
    private readonly ITransactionService _transactionService;

    public GetWalletTransactionsQueryHandler(IGenericRepository<Transaction> repository, IMapper mapper, IStringLocalizerFactory stringLocalizerFactory, ITransactionService transactionService)
    {
        _repository = repository;
        _mapper = mapper;
        _transactionService = transactionService;
        _tagLocalizer = stringLocalizerFactory.Create("TagTitles", "LinkPara.Emoney.API");
    }

    public async Task<PaginatedList<TransactionDto>> Handle(GetWalletTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _repository.GetAll()
            .Where(x => x.WalletId == request.WalletId);



        if (!string.IsNullOrEmpty(request.Q))
        {
            var tagTitles = _tagLocalizer.GetAllStrings().ToDictionary(x => x.Name, x => x.Value);
            var tagTitleMatch = tagTitles.FirstOrDefault(x => x.Value.ToLower().Contains(request.Q.ToLower()));
            if (tagTitleMatch.Key != null)
            {
                query = query.Where(x =>
                    x.TagTitle.ToLower().Contains(tagTitleMatch.Key.ToLower()) ||
                    x.Tag.ToLower().Contains(request.Q.ToLower()) ||
                    x.Amount.ToString().Contains(request.Q));
            }
            else
            {
                query = query.Where(x =>
                    x.Tag.ToLower().Contains(request.Q.ToLower()) ||
                    x.Amount.ToString().Contains(request.Q));
            }
        }
        else
        {
            query = query.Where(x =>
            x.TransactionDate >= request.StartDate &&
            x.TransactionDate <= request.EndDate);

            if (request.MinAmount > 0)
            {
                query = query.Where(x => x.Amount >= request.MinAmount);
            }

            if (request.MaxAmount > 0)
            {
                query = query.Where(x => x.Amount <= request.MaxAmount);
            }

            if (request.Direction.HasValue)
            {
                query = query.Where(x => x.TransactionDirection == request.Direction);
            }

            if (request.TransactionTypes != null && request.TransactionTypes.Length > 0)
            {
                query = query.Where(x => request.TransactionTypes.Contains(x.TransactionType));
            }

            if (request.OrderBy == OrderByStatus.Desc)
            {
                query = query.OrderByDescending(o => o.TransactionDate)
                .Select(x => new Transaction()
                {
                    Id = x.Id
                });
            }
            else
            {
                query = query.OrderBy(o => o.TransactionDate)
                .Select(x => new Transaction()
                {
                    Id = x.Id
                });
            }

        }
        var result = await query
                    .PaginatedListWithMappingAsync<Transaction, TransactionDto>(_mapper, request.Page, request.Size);


        var res = await _transactionService.GetWalletTransactionsWithDetailsAsync(result.Items.Select(x => x.Id).ToList(), cancellationToken);

        res.ForEach(p =>
        {
            p.TagTitle = !string.IsNullOrEmpty(p.TagTitle) ? _tagLocalizer.GetString(p.TagTitle) : p.TagTitle;
        });

        result.Items = res;

        return result;
    }
}