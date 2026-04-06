using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Consumers;

public class PostingPfProfitConsumer : IConsumer<PostingPfProfitControl>
{
    private readonly IGenericRepository<PostingPfProfit> _postingPfProfitRepository;
    private readonly IGenericRepository<PostingPfProfitDetail> _postingPfProfitDetailRepository;
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
    private readonly IGenericRepository<PostingTransaction> _postingTransactionRepository;
    private readonly IGenericRepository<Bank> _bankRepository;
    private readonly IApplicationUserService _applicationUserService;

    public PostingPfProfitConsumer(
        IGenericRepository<PostingPfProfit> postingPfProfitRepository, 
        IGenericRepository<PostingPfProfitDetail> postingPfProfitDetailRepository, 
        IGenericRepository<PostingBalance> postingBalanceRepository, 
        IGenericRepository<PostingTransaction> postingTransactionRepository, 
        IGenericRepository<Bank> bankRepository, 
        IApplicationUserService applicationUserService)
    {
        _postingPfProfitRepository = postingPfProfitRepository;
        _postingPfProfitDetailRepository = postingPfProfitDetailRepository;
        _postingBalanceRepository = postingBalanceRepository;
        _postingTransactionRepository = postingTransactionRepository;
        _bankRepository = bankRepository;
        _applicationUserService = applicationUserService;
    }

    public async Task Consume(ConsumeContext<PostingPfProfitControl> context)
    {
        var today = DateTime.Today;
        var createdBy = _applicationUserService.ApplicationUserId.ToString();
        var postingBalances = await _postingBalanceRepository.GetAll()
            .Where(x => x.PaymentDate == today)
            .GroupBy(b => new { b.PaymentDate, b.Currency })
            .Select(g => new PostingPfProfit
            {
                PaymentDate = g.Key.PaymentDate,
                Currency = g.Key.Currency,
                TotalPayingAmount = g.Sum(b => b.TotalPayingAmount),
                TotalPfNetCommissionAmount = g.Sum(b => b.TotalPfNetCommissionAmount)
            })
            .ToListAsync();

        var postingTransactions = await _postingTransactionRepository.GetAll()
            .Where(t => t.BankPaymentDate == today)
            .GroupBy(t => new { t.BankPaymentDate, t.AcquireBankCode, t.Currency })
            .Select(group => new PostingPfProfitDetail
            {
                AcquireBankCode = group.Key.AcquireBankCode,
                BankPayingAmount = group.Sum(t => t.AmountWithoutBankCommission),
                Currency = group.Key.Currency,
                CreatedBy = createdBy
            })
            .ToListAsync();

        var unmatchedCurrencies = postingTransactions
            .Select(t => t.Currency)
            .Except(postingBalances.Select(b => b.Currency))
            .Distinct()
            .ToList();

        var emptyPostingBalances = unmatchedCurrencies
            .Select(currency => new PostingPfProfit
            {
                PaymentDate = today,
                Currency = currency,
                TotalPayingAmount = 0.00m,
                TotalPfNetCommissionAmount = 0.00m
            })
            .ToList();

        postingBalances.AddRange(emptyPostingBalances);

        foreach (var postingBalance in postingBalances)
        {
            var postingTransactionsByCurrency
                = postingTransactions.Where(t => t.Currency == postingBalance.Currency).ToList();

            var amountWithoutBankCommission
                = Math.Round(postingTransactionsByCurrency.Sum(t => t.BankPayingAmount), 2);
            var totalPayingAmount = Math.Round(postingBalance.TotalPayingAmount, 2);
            var totalPfNetCommissionAmount = Math.Round(postingBalance.TotalPfNetCommissionAmount, 2);

            var postingPfProfit = new PostingPfProfit
            {
                PaymentDate = today,
                AmountWithoutBankCommission = amountWithoutBankCommission,
                TotalPayingAmount = totalPayingAmount,
                TotalPfNetCommissionAmount = totalPfNetCommissionAmount,
                ProtectionTransferAmount = Math.Round(amountWithoutBankCommission - totalPayingAmount - totalPfNetCommissionAmount, 2),
                CreateDate = DateTime.Now,
                Currency = postingBalance.Currency,
                RecordStatus = RecordStatus.Active,
                CreatedBy = createdBy
            };

            var createdPfProfit = await _postingPfProfitRepository.AddAsync(postingPfProfit);
            postingTransactionsByCurrency.ForEach(t => t.PostingPfProfitId = createdPfProfit.Id);
            foreach (var postingPfProfitDetail in postingTransactionsByCurrency)
            {
                var bank = await _bankRepository.GetAll()
                    .FirstOrDefaultAsync(b => b.Code == postingPfProfitDetail.AcquireBankCode);
                postingPfProfitDetail.BankName = bank?.Name;
            }
            await _postingPfProfitDetailRepository.AddRangeAsync(postingTransactionsByCurrency);
        }
    }
}