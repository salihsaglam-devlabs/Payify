using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class DepositLimitCheck : ILimitCheck
{
    private readonly IGenericRepository<Wallet> _walletRepository;

    public DepositLimitCheck(IGenericRepository<Wallet> walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<LimitControlResponse> CheckLimitAsync(LimitControlRequest request, TierLevel tierLevel,
        AccountCurrentLevel accountCurrentLevel)
    {
        if (!tierLevel.MaxDepositLimitEnabled)
        {
            return new LimitControlResponse
                { IsLimitExceeded = false, LimitOperationType = LimitOperationType.Deposit };
        }
        
        var mainWallet = await _walletRepository.GetAll()
            .Where(s => s.AccountId == request.AccountId
                        && s.IsMainWallet
                        && s.RecordStatus == RecordStatus.Active
                        && s.CurrencyCode == request.CurrencyCode)
            .SingleOrDefaultAsync();

        var totalBalance = await _walletRepository
            .GetAll()
            .Where(s => s.AccountId == request.AccountId
                        && s.RecordStatus == RecordStatus.Active
                        && s.CurrencyCode == request.CurrencyCode
                        && s.WalletType == mainWallet.WalletType)
            .SumAsync(sameTypeWallet => sameTypeWallet.CurrentBalanceCash + sameTypeWallet.CurrentBalanceCredit -
                                   sameTypeWallet.BlockedBalance);


        if (tierLevel.MaxBalanceLimitEnabled)
        {
            if (tierLevel.MaxBalance < totalBalance + request.Amount)
            {
                return new LimitControlResponse
                {
                    IsLimitExceeded = true,
                    LimitOperationType = LimitOperationType.MaxBalance
                };
            }    
        }
        
        var isLimitExceeded = tierLevel.DailyMaxDepositAmount < accountCurrentLevel.DailyDepositAmount + request.Amount
                              || tierLevel.MonthlyMaxDepositAmount < accountCurrentLevel.MonthlyDepositAmount + request.Amount
                              || tierLevel.DailyMaxDepositCount <= accountCurrentLevel.DailyDepositCount
                              || tierLevel.MonthlyMaxDepositCount <= accountCurrentLevel.MonthlyDepositCount;

        return new LimitControlResponse
            { IsLimitExceeded = isLimitExceeded, LimitOperationType = LimitOperationType.Deposit };
    }
}