using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;

namespace LinkPara.PF.Application.Features.PostingBalances.Queries.GetPostingBalanceStatus;

public class GetPostingBalanceStatusQuery : IRequest<List<PostingBalanceStatusModel>>
{
    public Guid? MerchantId { get; set; }
    public DateTime? TransactionDateStart { get; set; }
    public DateTime? TransactionDateEnd { get; set; }
}

public class GetPostingBalanceStatusQueryHandler : IRequestHandler<GetPostingBalanceStatusQuery, List<PostingBalanceStatusModel>>
{
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;

    public GetPostingBalanceStatusQueryHandler(IGenericRepository<PostingBalance> postingBalanceRepository)
    {
        _postingBalanceRepository = postingBalanceRepository;
    }
    public async Task<List<PostingBalanceStatusModel>> Handle(GetPostingBalanceStatusQuery request, CancellationToken cancellationToken)
    {
        var allPostingBalances = _postingBalanceRepository.GetAll();

        if (request.MerchantId is not null)
        {
            allPostingBalances = allPostingBalances.Where(b => b.MerchantId
                               == request.MerchantId);
        }

        if (request.TransactionDateStart is not null)
        {
            allPostingBalances = allPostingBalances.Where(b => b.TransactionDate
                               >= request.TransactionDateStart);
        }

        if (request.TransactionDateEnd is not null)
        {
            allPostingBalances = allPostingBalances.Where(b => b.TransactionDate
                               <= request.TransactionDateEnd);
        }

        var totalCount = await allPostingBalances.CountAsync(cancellationToken);

        var statusList = new List<PostingBalanceStatusModel>();
       
        var pendingCount = 0;

        foreach (PostingMoneyTransferStatus enumValue in Enum.GetValues(typeof(PostingMoneyTransferStatus)))
        {
            pendingCount = await allPostingBalances
            .Where(b => b.MoneyTransferStatus == enumValue).CountAsync(cancellationToken);

            statusList.Add(new PostingBalanceStatusModel
            {
                Status = enumValue.ToString(),
                Count = pendingCount,
                Percent = (double)(pendingCount * 100) / totalCount
            });
        }

        return statusList;
    }
}