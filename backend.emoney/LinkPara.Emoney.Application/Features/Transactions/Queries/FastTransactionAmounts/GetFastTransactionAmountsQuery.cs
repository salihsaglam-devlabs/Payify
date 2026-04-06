using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Features.Currencies;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetFastTransactionAmountsQuery;

public class GetFastTransactionAmountsQuery : IRequest<FastTransactionAmountsDto>
{
    public Guid WalletId { get; set; }
}

public class GetFastTransactionAmountsQueryHandler : IRequestHandler<GetFastTransactionAmountsQuery, FastTransactionAmountsDto>
{
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;

    public GetFastTransactionAmountsQueryHandler(IGenericRepository<Transaction> transactionRepository, IGenericRepository<Wallet> walletRepository)
    {
        _transactionRepository = transactionRepository;
        _walletRepository = walletRepository;
    }

    public async Task<FastTransactionAmountsDto> Handle(GetFastTransactionAmountsQuery request,
        CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetAll()
            .Where(t => t.WalletId == request.WalletId &&
            t.TransactionDirection == TransactionDirection.MoneyOut &&
            t.TransactionType == TransactionType.Withdraw &&
            (PaymentMethod.BankTransfer == t.PaymentMethod ||
            PaymentMethod.Transfer == t.PaymentMethod) &&
            t.TransactionStatus == TransactionStatus.Completed)
            .OrderByDescending(t => t.TransactionDate)
            .Take(10)
            .ToListAsync(cancellationToken);

        var wallet = await _walletRepository.GetByIdAsync(request.WalletId);
        if (wallet == null)
        {
            throw new NotFoundException($"Wallet with ID {request.WalletId} not found.");
        }

        if (!transactions.Any())
        {
            return new FastTransactionAmountsDto
            {
                MostTransactionAmount = 0,
                LastTransactionAmount = 0,
                UserBalanceAmount = wallet.CurrentBalanceCash
            };
        }

        var amountGroups = transactions
            .GroupBy(t => t.Amount)
            .Select(g => new { Amount = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Amount)
            .ToList();

        var suggestedAmount = amountGroups.First().Amount;

        return new FastTransactionAmountsDto
        {
            MostTransactionAmount = suggestedAmount,
            LastTransactionAmount = transactions.First().Amount,
            UserBalanceAmount = wallet.CurrentBalanceCash
        };
    }
}