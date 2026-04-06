using AutoMapper;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Transactions;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Emoney.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Transaction> _repository;
    private readonly IStringLocalizer _tagLocalizer;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<WithdrawRequest> _withdrawRequestRepository;


    public TransactionService(
        IMapper mapper, 
        IStringLocalizerFactory stringLocalizerFactory, 
        IGenericRepository<WithdrawRequest> withdrawRequestRepository, 
        IGenericRepository<Transaction> repository, 
        IGenericRepository<Wallet> walletRepository)
    {
        _mapper = mapper;
        _withdrawRequestRepository = withdrawRequestRepository;
        _repository = repository;
        _walletRepository = walletRepository;
        _tagLocalizer= stringLocalizerFactory.Create("TagTitles", "LinkPara.Emoney.API");
    }

    public async Task<TransactionDto> GetTransactionWithDetailsAsync(Guid transactionId, CancellationToken cancellationToken)
    {
        var result =  await GetTransactionsAsync(new List<Guid> { transactionId }, cancellationToken);
        
        return result.FirstOrDefault();
    }
    
    public async Task<List<TransactionDto>> GetWalletTransactionsWithDetailsAsync(List<Guid> transactionIds, CancellationToken cancellationToken)
    {
        var result = await GetTransactionsAsync(transactionIds, cancellationToken);
        
        return result;
    }
    
    private async Task<List<TransactionDto>> GetTransactionsAsync(List<Guid> transactionIds, CancellationToken cancellationToken)
    {
        var transactions = new List<TransactionDto>();
        
        foreach (var transactionId in transactionIds)
        {
            var transaction = await _repository.GetAll(q => q.Currency)
                .Include(x => x.Wallet)
                .ThenInclude(s => s.Account)
                .SingleOrDefaultAsync(x => x.Id == transactionId, cancellationToken);
            
            if (transaction is null)
            {
                throw new NotFoundException(nameof(Transaction), transactionId);
            }

            var transactionDto = _mapper.Map<TransactionDto>(transaction);

            transactionDto.WalletNumber = transaction.Wallet.WalletNumber;
            transactionDto.WalletName = transaction.Wallet.Account.Name;

            if (transaction.CounterWalletId != null)
            {
                var counterWallet = await _walletRepository.GetAll()
                    .Include(x => x.Account)
                    .Select(x => new
                    {
                        x.Id,
                        x.WalletNumber,
                        x.Account.Name
                    }).FirstOrDefaultAsync(x => x.Id == transaction.CounterWalletId,
                        cancellationToken: cancellationToken);

                transactionDto.CounterWalletNumber = counterWallet?.WalletNumber;
                transactionDto.CounterWalletName = counterWallet?.Name;
            }

            if (transaction.WithdrawRequestId is not null)
            {
                var withdrawRequest = await _withdrawRequestRepository.GetAll()
                    .Select(x => new
                    {
                        x.Id,
                        x.ReceiverIbanNumber
                    }).FirstOrDefaultAsync(x => x.Id == transaction.WithdrawRequestId,
                        cancellationToken: cancellationToken);
                transactionDto.ReceiverIban = withdrawRequest?.ReceiverIbanNumber;
            }

            var relatedTransactions = await _repository.GetAll()
                .Where(s => s.RelatedTransactionId == transaction.Id).ToListAsync(cancellationToken: cancellationToken);

            if (relatedTransactions.Count > 0)
            {
                var bsmv = relatedTransactions.Select(x => new
                {
                    x.Tag,
                    x.Amount,
                    x.TransactionType
                }).FirstOrDefault(s => s.TransactionType == TransactionType.Tax);

                var pricing = relatedTransactions.Select(x => new
                {
                    x.Tag,
                    x.Amount,
                    x.TransactionType
                }).FirstOrDefault(s => s.TransactionType == TransactionType.Commission);

                transactionDto.TaxAmount = bsmv?.Amount ?? 0;
                transactionDto.CommissionAmount = pricing?.Amount ?? 0;
            }

            transactionDto.Amount = transactionDto.TransactionType == TransactionType.Deposit &&
                                         transactionDto.PaymentMethod == PaymentMethod.CreditCard 
                        ? transactionDto.Amount - (transactionDto.TaxAmount + transactionDto.CommissionAmount) : transactionDto.Amount;

            transactionDto.TotalAmount = transactionDto.TransactionType == TransactionType.Deposit &&
                                            transactionDto.PaymentMethod != PaymentMethod.CreditCard
                ? transactionDto.Amount - (transactionDto.TaxAmount + transactionDto.CommissionAmount)
                : transactionDto.Amount + (transactionDto.TaxAmount + transactionDto.CommissionAmount);

            transactionDto.TagTitle = !string.IsNullOrEmpty(transactionDto.TagTitle)
                ? _tagLocalizer.GetString(transactionDto.TagTitle)
                : transactionDto.TagTitle;

            transactionDto.TotalAmountText = AmountTextConverter.DecimalToWords(transactionDto.TotalAmount);
            transactions.Add(transactionDto);
        }

        return transactions;
    }
}
