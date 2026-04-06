using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class InternalTransferLimitCheck : ILimitCheck
{
    private readonly IGenericRepository<Wallet> _walletRepository;

    public InternalTransferLimitCheck(IGenericRepository<Wallet> walletRepository)
    {
        _walletRepository = walletRepository;
    }

    public async Task<LimitControlResponse> CheckLimitAsync(LimitControlRequest request, TierLevel tierLevel,
        AccountCurrentLevel accountCurrentLevel)
    {
        if (!tierLevel.MaxInternalTransferLimitEnabled)
        {
            return new LimitControlResponse
            {
                IsLimitExceeded = false,
                LimitOperationType = LimitOperationType.InternalTransfer
            };
        }

        var sourceWallet = await _walletRepository
            .GetAll(s => s.Currency)
            .Where(s => s.AccountId == request.AccountId)
            .Where(s => s.RecordStatus == RecordStatus.Active)
            .Where(s => s.Currency.Code == request.CurrencyCode)
            .SingleOrDefaultAsync(s => s.WalletNumber == request.WalletNumber);

        if (sourceWallet is null)
        {
            throw new NotFoundException(nameof(request.WalletNumber), request.WalletNumber);
        }

        var isExceeded  = tierLevel.DailyMaxInternalTransferAmount < accountCurrentLevel.DailyInternalTransferAmount + request.Amount
                     || tierLevel.MonthlyMaxInternalTransferAmount < accountCurrentLevel.MonthlyInternalTransferAmount + request.Amount
                     || tierLevel.DailyMaxInternalTransferCount <= accountCurrentLevel.DailyInternalTransferCount
                     || tierLevel.MonthlyMaxInternalTransferCount <= accountCurrentLevel.MonthlyInternalTransferCount;

        return new LimitControlResponse
        {
            IsLimitExceeded = isExceeded,
            LimitOperationType = LimitOperationType.InternalTransfer
        };
    }
}