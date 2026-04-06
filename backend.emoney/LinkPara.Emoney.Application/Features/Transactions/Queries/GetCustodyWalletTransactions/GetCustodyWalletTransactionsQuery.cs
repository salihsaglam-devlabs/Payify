using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetWalletTransactions;

public class GetCustodyWalletTransactionsQuery : SearchQueryParams, IRequest<PaginatedList<TransactionDto>>
{
    public Guid ParentAccountId { get; set; }
    public Guid WalletId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Now.AddDays(-30).Add(-DateTime.Now.TimeOfDay);
    public DateTime EndDate { get; set; } = DateTime.Now;
}

public class GetCustodyWalletTransactionsQueryHandler : IRequestHandler<GetCustodyWalletTransactionsQuery, PaginatedList<TransactionDto>>
{
    private readonly IGenericRepository<Transaction> _repository;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _tagLocalizer;

    private readonly IStringLocalizer _localizer;

    public GetCustodyWalletTransactionsQueryHandler(IGenericRepository<Transaction> repository, 
        IMapper mapper, 
        IStringLocalizerFactory stringLocalizerFactory,
        IStringLocalizerFactory factory)
    {
        _repository = repository;
        _mapper = mapper;
        _tagLocalizer = stringLocalizerFactory.Create("TagTitles", "LinkPara.Emoney.API");
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");
    }

    public async Task<PaginatedList<TransactionDto>> Handle(GetCustodyWalletTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _repository.GetAll()
            .Include(x => x.Wallet)
            .ThenInclude(x => x.Account)
            .Where(x => x.WalletId == request.WalletId 
                     && x.RecordStatus == RecordStatus.Active);

        if (query.Count() > 0)
        {
            if (query.FirstOrDefault().Wallet.Account.ParentAccountId != request.ParentAccountId)
            {
                var exceptionMessage = _localizer.GetString("ParentAccountNotRelativeWithWallet").Value;
                throw new Exception(exceptionMessage);
            }


            if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where(x => x.Tag.ToLower().Contains(request.Q.ToLower()) || x.Amount.ToString().Contains(request.Q));
            }
            else
            {
                query = query.Where(x =>
                    x.TransactionDate >= request.StartDate &&
                    x.TransactionDate <= request.EndDate)
                    .OrderByDescending(o => o.TransactionDate);
            }
        }
        
        var result = await query
            .PaginatedListWithMappingAsync<Transaction,TransactionDto>(_mapper, request.Page, request.Size);
        result.Items.ForEach(p =>
        {
            p.TagTitle = !string.IsNullOrEmpty(p.TagTitle) ? _tagLocalizer.GetString(p.TagTitle) : p.TagTitle;
        });
        return result;
    }
}