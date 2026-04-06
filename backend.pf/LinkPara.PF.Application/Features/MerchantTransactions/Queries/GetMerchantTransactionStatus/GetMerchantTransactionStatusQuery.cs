using LinkPara.PF.Application.Commons.Models.MerchantTransactions;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetMerchantTransactionStatus;

public class GetMerchantTransactionStatusQuery : IRequest<List<MerchantTransactionStatusModel>>
{
    public Guid? MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}

public class GetMerchantTransactionStatusQueryHandler : IRequestHandler<GetMerchantTransactionStatusQuery, List<MerchantTransactionStatusModel>>
{
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;

    public GetMerchantTransactionStatusQueryHandler(IGenericRepository<MerchantTransaction> merchantTransactionRepository)
    {
        _merchantTransactionRepository = merchantTransactionRepository;
    }
    public async Task<List<MerchantTransactionStatusModel>> Handle(GetMerchantTransactionStatusQuery request, CancellationToken cancellationToken)
    {
        var allTransactions = _merchantTransactionRepository.GetAll();

        if (request.MerchantId is not null)
        {
            allTransactions = allTransactions.Where(b => b.MerchantId == request.MerchantId);
        }

        if (request.SubMerchantId is not null)
        {
            allTransactions = allTransactions.Where(b => b.SubMerchantId == request.SubMerchantId);
        }

        if (request.CreateDateStart is not null)
        {
            allTransactions = allTransactions.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            allTransactions = allTransactions.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        var transactions = await allTransactions.GroupBy(b => b.TransactionStatus)
            .Select(g => new MerchantTransactionStatusModel { TransactionStatus = g.Key, Count = g.Count() }).ToListAsync(cancellationToken);

        var totalCount = await allTransactions.CountAsync(cancellationToken);

        foreach (var item in transactions)
        {
            item.Percent = (double)(item.Count * 100) / totalCount;
        }

        return transactions;
    }
}
