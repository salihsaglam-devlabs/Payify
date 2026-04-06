using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class MaxBalanceLimitCheck : ILimitCheck
{
    private readonly IGenericRepository<Wallet> _walletRepository;

    public MaxBalanceLimitCheck(IGenericRepository<Wallet> walletRepository)
    {
        _walletRepository = walletRepository;
    }
    
    public async Task<LimitControlResponse> CheckLimitAsync(LimitControlRequest request, TierLevel tierLevel, AccountCurrentLevel accountCurrentLevel)
    {
        if (!tierLevel.MaxBalanceLimitEnabled)
        {
            return new LimitControlResponse
            {
                IsLimitExceeded = false,
                LimitOperationType = LimitOperationType.MaxBalance
            };
        }
        
        var wallet = await _walletRepository.GetAll()
            .Where(s => s.WalletNumber == request.WalletNumber)
            .SingleOrDefaultAsync();

        if (wallet is null)
        {
            throw new NotFoundException(nameof(request.WalletNumber), request.WalletNumber);
        }

        var sameTypeWallets = await _walletRepository.GetAll()
            .Where(s => s.AccountId == request.AccountId
                        && s.RecordStatus == RecordStatus.Active
                        && s.CurrencyCode == request.CurrencyCode
                        && s.WalletType == wallet.WalletType).ToListAsync();

        var totalBalance = sameTypeWallets
            .Sum(sameTypeWallet => sameTypeWallet.CurrentBalanceCash + sameTypeWallet.CurrentBalanceCredit -
                                   sameTypeWallet.BlockedBalance);

        var isExceeded = totalBalance + request.Amount > tierLevel.MaxBalance;
        
        return new LimitControlResponse
        {
            IsLimitExceeded = isExceeded,
            LimitOperationType = LimitOperationType.MaxBalance
        };
    }
}