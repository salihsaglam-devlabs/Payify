using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetAccountCurrentLimits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierPermissionsQuery;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class LimitService : ILimitService
{
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ITierLevelService _tierLevelService;
    private readonly IGenericRepository<TierLevel> _tierLevelRepository;
    private readonly IGenericRepository<AccountCustomTier> _accountCustomTierLevelRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly ITierPermissionService _tierPermissionService;

    public LimitService(
        IGenericRepository<Wallet> walletRepository,
        IAuditLogService auditLogService,
        ITierLevelService tierLevelService,
        IGenericRepository<TierLevel> tierLevelRepository,
        IGenericRepository<AccountCustomTier> accountCustomTierLevelRepository,
        IGenericRepository<Account> accountRepository,
        ITierPermissionService tierPermissionService)
    {
        _walletRepository = walletRepository;
        _auditLogService = auditLogService;
        _tierLevelService = tierLevelService;
        _tierLevelRepository = tierLevelRepository;
        _accountCustomTierLevelRepository = accountCustomTierLevelRepository;
        _accountRepository = accountRepository;
        _tierPermissionService = tierPermissionService;
    }

    public async Task<LimitControlResponse> IsLimitExceededAsync(LimitControlRequest request)
    {
        var accountCurrentLevel = await _tierLevelService.FindAccountCurrentLevel(request.AccountId, request.CurrencyCode);
        var tierLevel = await _tierLevelService.FindTierLevelAsync(request.AccountId);
        var checker = GetChecker(request.LimitOperationType);
        var result = await checker.CheckLimitAsync(request, tierLevel, accountCurrentLevel);

        if (result.IsLimitExceeded)
        {
            await LimitResponseAuditLogAsync(true, request.AccountId, new Dictionary<string, string>
            {
                {"LimitOperationType",request.LimitOperationType.ToString() },
                {"IsLimitExeeded",result.IsLimitExceeded.ToString() }
            });
        }

        return result;
    }

    private ILimitCheck GetChecker(LimitOperationType limitOperationType)
    {
        return limitOperationType switch
        {
            LimitOperationType.Deposit => new DepositLimitCheck(_walletRepository),
            LimitOperationType.Withdrawal => new WithdrawLimitCheck(),
            LimitOperationType.InternalTransfer => new InternalTransferLimitCheck(_walletRepository),
            LimitOperationType.InternationalTransfer => new InternationalTransferLimitCheck(),
            LimitOperationType.MaxBalance => new MaxBalanceLimitCheck(_walletRepository),
            LimitOperationType.WithdrawIban => new WithdrawIbanLimitCheck(),
            LimitOperationType.OnUs => new OnUsPaymentLimitCheck(),
            _ => throw new ArgumentOutOfRangeException(nameof(limitOperationType)),
        };
    }

    public Task IncreaseUsageAsync(AccountLimitUpdateRequest request, AccountCurrentLevel currentLevel)
    {
        if (currentLevel is null)
            return Task.CompletedTask;

        var today = DateTime.Now;

        if (today.Day == currentLevel.LevelDate.Day &&
            today.Month == currentLevel.LevelDate.Month &&
            today.Year == currentLevel.LevelDate.Year)
        {
            IncreaseUsage(request, currentLevel);
            return Task.CompletedTask;
        }

        if (today.Year != currentLevel.LevelDate.Year
            || today.Month != currentLevel.LevelDate.Month)
        {
            ResetDailyUsage(currentLevel);
            ResetMonthlyUsage(currentLevel);
            IncreaseUsage(request, currentLevel);
            currentLevel.LevelDate = today;
            return Task.CompletedTask;
        }

        if (today.Day != currentLevel.LevelDate.Day
            && today.Month == currentLevel.LevelDate.Month)
        {
            ResetDailyUsage(currentLevel);
            IncreaseUsage(request, currentLevel);
            currentLevel.LevelDate = today;
        }

        return Task.CompletedTask;
    }

    private static void ResetDailyUsage(AccountCurrentLevel level)
    {
        level.DailyInternalTransferAmount = 0;
        level.DailyInternalTransferCount = 0;
        level.DailyInternationalTransferAmount = 0;
        level.DailyInternationalTransferCount = 0;
        level.DailyDepositAmount = 0;
        level.DailyDepositCount = 0;
        level.DailyWithdrawalAmount = 0;
        level.DailyWithdrawalCount = 0;
        level.DailyOwnIbanWithdrawalCount = 0;
        level.DailyOtherIbanWithdrawalCount = 0;
        level.DailyDistinctOtherIbanWithdrawalCount = 0;
        level.DailyOtherIbanWithdrawalAmount = 0;
        level.DailyOnUsPaymentAmount = 0;
        level.DailyOnUsPaymentCount = 0;
    }

    private static void ResetMonthlyUsage(AccountCurrentLevel level)
    {
        level.MonthlyDepositAmount = 0;
        level.MonthlyDepositCount = 0;
        level.MonthlyWithdrawalAmount = 0;
        level.MonthlyWithdrawalCount = 0;
        level.MonthlyInternalTransferAmount = 0;
        level.MonthlyInternalTransferCount = 0;
        level.MonthlyInternationalTransferAmount = 0;
        level.MonthlyInternationalTransferCount = 0;
        level.MonthlyOwnIbanWithdrawalCount = 0;
        level.MonthlyOtherIbanWithdrawalCount = 0;
        level.MonthlyDistinctOtherIbanWithdrawalCount = 0;
        level.MonthlyOtherIbanWithdrawalAmount = 0;
        level.MonthlyOnUsPaymentAmount = 0;
        level.MonthlyOnUsPaymentCount = 0;
    }

    private static void IncreaseUsage(AccountLimitUpdateRequest request, AccountCurrentLevel level)
    {
        if (request.LimitOperationType == LimitOperationType.Deposit)
        {
            level.DailyDepositAmount += request.Amount;
            level.MonthlyDepositAmount += request.Amount;
            level.DailyDepositCount += 1;
            level.MonthlyDepositCount += 1;
        }
        else if (request.LimitOperationType == LimitOperationType.Withdrawal)
        {
            level.DailyWithdrawalAmount += request.Amount;
            level.MonthlyWithdrawalAmount += request.Amount;
            level.DailyWithdrawalCount += 1;
            level.MonthlyWithdrawalCount += 1;
            if (request.IsOwnIban.HasValue && !(bool)request.IsOwnIban)
            {
                level.DailyOtherIbanWithdrawalCount += 1;
                if (request.IsDailyDistinctIban.HasValue && (bool)request.IsDailyDistinctIban)
                {
                    level.DailyDistinctOtherIbanWithdrawalCount += 1;
                }
                level.DailyOtherIbanWithdrawalAmount += request.Amount;
                level.MonthlyOtherIbanWithdrawalCount += 1;
                if (request.IsMonthlyDistinctIban.HasValue && (bool)request.IsMonthlyDistinctIban)
                {
                    level.MonthlyDistinctOtherIbanWithdrawalCount += 1;
                }
                level.MonthlyOtherIbanWithdrawalAmount += request.Amount;
            }
            if (request.IsOwnIban.HasValue && (bool)request.IsOwnIban)
            {
                level.DailyOwnIbanWithdrawalCount += 1;
                level.MonthlyOwnIbanWithdrawalCount += 1;
            }
        }
        else if (request.LimitOperationType == LimitOperationType.InternalTransfer)
        {
            level.DailyInternalTransferAmount += request.Amount;
            level.MonthlyInternalTransferAmount += request.Amount;
            level.DailyInternalTransferCount += 1;
            level.MonthlyInternalTransferCount += 1;
        }
        else if (request.LimitOperationType == LimitOperationType.InternationalTransfer)
        {
            level.DailyInternalTransferAmount += request.Amount;
            level.MonthlyInternationalTransferAmount += request.Amount;
            level.DailyInternalTransferCount += 1;
            level.MonthlyInternationalTransferCount += 1;
        }
        else if (request.LimitOperationType == LimitOperationType.OnUs)
        {
            level.DailyOnUsPaymentAmount += request.Amount;
            level.MonthlyOnUsPaymentAmount += request.Amount;
            level.DailyOnUsPaymentCount += 1;
            level.MonthlyOnUsPaymentCount += 1;
        }
    }

    private async Task LimitResponseAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> details)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "IsLimitExceededAsync",
                Resource = "LimitService",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }

    public Task DecreaseUsageAsync(AccountLimitUpdateRequest request, AccountCurrentLevel currentLevel)
    {
        var today = DateTime.Now;

        if (currentLevel.LevelDate.Day == today.Day &&
            currentLevel.LevelDate.Month == today.Month &&
            currentLevel.LevelDate.Year == today.Year)
        {
            DecreaseDailyUsage(request, currentLevel);
        }

        if (currentLevel.LevelDate.Day != today.Day &&
            currentLevel.LevelDate.Month == today.Month &&
            currentLevel.LevelDate.Year == today.Year)
        {
            DecreaseMonthlyUsage(request, currentLevel);
        }

        return Task.CompletedTask;
    }

    private void DecreaseDailyUsage(AccountLimitUpdateRequest request, AccountCurrentLevel level)
    {
        if (request.LimitOperationType == LimitOperationType.Deposit)
        {
            level.DailyDepositAmount -= request.Amount;
            level.MonthlyDepositAmount -= request.Amount;
            level.DailyDepositCount -= 1;
            level.MonthlyDepositCount -= 1;
        }
        else if (request.LimitOperationType == LimitOperationType.Withdrawal)
        {
            level.DailyWithdrawalAmount -= request.Amount;
            level.MonthlyWithdrawalAmount -= request.Amount;
            level.DailyWithdrawalCount -= 1;
            level.MonthlyWithdrawalCount -= 1;
            if (request.IsOwnIban.HasValue && !(bool)request.IsOwnIban)
            {
                level.DailyOtherIbanWithdrawalCount -= 1;
                if (request.IsDailyDistinctIban.HasValue && (bool)request.IsDailyDistinctIban)
                {
                    level.DailyDistinctOtherIbanWithdrawalCount -= 1;
                }
                level.DailyOtherIbanWithdrawalAmount -= request.Amount;
                level.MonthlyOtherIbanWithdrawalCount -= 1;
                if (request.IsMonthlyDistinctIban.HasValue && (bool)request.IsMonthlyDistinctIban)
                {
                    level.MonthlyDistinctOtherIbanWithdrawalCount -= 1;
                }
                level.MonthlyOtherIbanWithdrawalAmount -= request.Amount;
            }
            if (request.IsOwnIban.HasValue && (bool)request.IsOwnIban)
            {
                level.DailyOwnIbanWithdrawalCount -= 1;
                level.MonthlyOwnIbanWithdrawalCount -= 1;
            }
        }
        else if (request.LimitOperationType == LimitOperationType.InternalTransfer)
        {
            level.DailyInternalTransferAmount -= request.Amount;
            level.MonthlyInternalTransferAmount -= request.Amount;
            level.DailyInternalTransferCount -= 1;
            level.MonthlyInternalTransferCount -= 1;
        }
        else if (request.LimitOperationType == LimitOperationType.InternationalTransfer)
        {
            level.DailyInternalTransferAmount -= request.Amount;
            level.MonthlyInternationalTransferAmount -= request.Amount;
            level.DailyInternalTransferCount -= 1;
            level.MonthlyInternationalTransferCount -= 1;
        }
        else if (request.LimitOperationType == LimitOperationType.OnUs)
        {
            level.DailyOnUsPaymentAmount -= request.Amount;
            level.MonthlyOnUsPaymentAmount -= request.Amount;
            level.DailyOnUsPaymentCount -= 1;
            level.MonthlyOnUsPaymentCount -= 1;

            level.DailyOnUsPaymentAmount = (level.DailyOnUsPaymentAmount < 0) ? 0 : level.DailyOnUsPaymentAmount;
            level.DailyOnUsPaymentCount = (level.DailyOnUsPaymentCount < 0) ? 0 : level.DailyOnUsPaymentCount;
            level.MonthlyOnUsPaymentAmount = (level.MonthlyOnUsPaymentAmount < 0) ? 0 : level.MonthlyOnUsPaymentAmount;
            level.MonthlyOnUsPaymentCount = (level.MonthlyOnUsPaymentCount < 0) ? 0 : level.MonthlyOnUsPaymentCount;
        }
    }

    private void DecreaseMonthlyUsage(AccountLimitUpdateRequest request, AccountCurrentLevel level)
    {
        if (request.LimitOperationType == LimitOperationType.Deposit)
        {
            level.MonthlyDepositAmount -= request.Amount;
            level.MonthlyDepositCount -= 1;
        }
        else if (request.LimitOperationType == LimitOperationType.Withdrawal)
        {
            level.MonthlyWithdrawalAmount -= request.Amount;
            level.MonthlyWithdrawalCount -= 1;
            if (request.IsOwnIban is not null && !(bool)request.IsOwnIban)
            {
                level.MonthlyOtherIbanWithdrawalCount -= 1;
                if (request.IsMonthlyDistinctIban.HasValue && (bool)request.IsMonthlyDistinctIban)
                {
                    level.MonthlyDistinctOtherIbanWithdrawalCount -= 1;
                }
                level.MonthlyOtherIbanWithdrawalAmount -= request.Amount;
            }
            if (request.IsOwnIban is not null && (bool)request.IsOwnIban)
            {
                level.MonthlyOwnIbanWithdrawalCount -= 1;
            }
        }
        else if (request.LimitOperationType == LimitOperationType.InternalTransfer)
        {
            level.MonthlyInternalTransferAmount -= request.Amount;
            level.MonthlyInternalTransferCount -= 1;
        }
        else if (request.LimitOperationType == LimitOperationType.InternationalTransfer)
        {
            level.MonthlyInternationalTransferAmount -= request.Amount;
            level.MonthlyInternationalTransferCount -= 1;
        }
        if (request.LimitOperationType == LimitOperationType.OnUs)
        {
            level.MonthlyOnUsPaymentAmount -= request.Amount;
            level.MonthlyOnUsPaymentCount -= 1;

            level.MonthlyOnUsPaymentAmount = (level.MonthlyOnUsPaymentAmount < 0) ? 0 : level.MonthlyOnUsPaymentAmount;
            level.MonthlyOnUsPaymentCount = (level.MonthlyOnUsPaymentCount < 0) ? 0 : level.MonthlyOnUsPaymentCount;
        }
    }

    public async Task<AccountLimitDto> GetAccountLimitsQuery(GetAccountLimitsQuery request)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId);

        if (account == null)
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        TierLevel tierLevel;

        var customTierLevel = await _accountCustomTierLevelRepository.GetAll()
           .Include(x => x.TierLevel)
           .ThenInclude(x => x.Currency)
           .FirstOrDefaultAsync(x => x.AccountId == request.AccountId &&
           x.RecordStatus == RecordStatus.Active);

        tierLevel = customTierLevel?.TierLevel;

        if (tierLevel is null)
        {
            tierLevel = await _tierLevelRepository.GetAll(q => q.Currency)
                .SingleOrDefaultAsync(f => f.TierLevelType == GetTierLevelType(account.AccountKycLevel));

            if (tierLevel is null)
            {
                throw new NotFoundException(nameof(TierLevel), account.AccountKycLevel);
            }
        }

        var accountCurrentLevel = await _tierLevelService.FindAccountCurrentLevel(account.Id, request.CurrencyCode);

        if (accountCurrentLevel is null)
        {
            throw new NotFoundException(nameof(AccountCurrentLevel), account.Id);
        }

        var currentUsageSummary = new AccountLimitDto
        {
            CurrencySymbol = tierLevel.Currency.Symbol,
            AccountId = request.AccountId,
            TierLevelType = tierLevel.TierLevelType,
            Name = tierLevel.Name,
            IndividualWallet = await PopulateBalanceLimitAsync(tierLevel, request.AccountId, WalletType.Individual),
            Deposit = await PopulateDepositLimitAsync(tierLevel, accountCurrentLevel),
            Withdraw = await PopulateWithdrawLimitAsync(tierLevel, accountCurrentLevel),
            WithdrawToOwnIban = await PopulateWithdrawToOwnIbanAsync(tierLevel, accountCurrentLevel),
            WithdrawToOtherIban = await PopulateWithdrawToOtherIbanAsync(tierLevel, accountCurrentLevel),
            InternalTransfer = await PopulateInternalTransferLimitAsync(tierLevel, accountCurrentLevel),
            InternationalTransfer = await PopulateInternationalTransferLimitAsync(tierLevel, accountCurrentLevel),
            TierPermissions = await PopulateTierPermissionsAsync(tierLevel),
            TierLevelUpgradePaths = await PopulateTierLevelUpgradePathsAsync(tierLevel),
            OnUsPayment = await PopulateOnUsPaymentLimitAsync(tierLevel, accountCurrentLevel)
        };

        return currentUsageSummary;
    }

    private static TierLevelType GetTierLevelType(AccountKycLevel accountKycLevel)
    {
        return accountKycLevel switch
        {
            AccountKycLevel.NoneKyc => TierLevelType.Tier0,
            AccountKycLevel.PreKyc => TierLevelType.Tier1,
            AccountKycLevel.Kyc => TierLevelType.Tier2,
            AccountKycLevel.Premium => TierLevelType.Tier3,
            AccountKycLevel.PremiumPlus => TierLevelType.Tier4,
            AccountKycLevel.ChildKyc => TierLevelType.Tier5,
            AccountKycLevel.CorporateKyc => TierLevelType.Corporate,
            _ => throw new ArgumentOutOfRangeException(nameof(accountKycLevel))
        };
    }

    private async Task<BalanceLimitDto> PopulateBalanceLimitAsync(TierLevel tierLevel, Guid accountId, WalletType walletType)
    {
        var balance = await GetBalanceAsync(accountId, tierLevel.CurrencyCode, walletType);

        return new BalanceLimitDto
        {
            MaxBalanceLimitEnabled = tierLevel.MaxBalanceLimitEnabled,
            CurrentBalance = balance,
            MaxBalance = tierLevel.MaxBalance
        };
    }

    private async Task<decimal> GetBalanceAsync(Guid accountId, string currencyCode, WalletType walletType)
    {
        var sameTypeWallets = await _walletRepository.GetAll()
            .Where(s => s.AccountId == accountId
                        && s.RecordStatus == RecordStatus.Active
                        && s.CurrencyCode == currencyCode
                        && s.WalletType == walletType).ToListAsync();

        var totalBalance = sameTypeWallets
            .Sum(sameTypeWallet => sameTypeWallet.CurrentBalanceCash + sameTypeWallet.CurrentBalanceCredit -
                                   sameTypeWallet.BlockedBalance);

        return totalBalance;
    }

    private static Task<UsageLimitDto> PopulateDepositLimitAsync(TierLevel tierLevel, AccountCurrentLevel accountCurrentLevel)
    {
        return Task.FromResult(new UsageLimitDto
        {
            MaxLimitEnabled = tierLevel.MaxDepositLimitEnabled,
            DailyMaxAmount = tierLevel.DailyMaxDepositAmount,
            DailyMaxCount = tierLevel.DailyMaxDepositCount,
            DailyUserAmount = accountCurrentLevel.DailyDepositAmount,
            DailyUserCount = accountCurrentLevel.DailyDepositCount,
            MonthlyMaxAmount = tierLevel.MonthlyMaxDepositAmount,
            MonthlyMaxCount = tierLevel.MonthlyMaxDepositCount,
            MonthlyUserAmount = accountCurrentLevel.MonthlyDepositAmount,
            MonthlyUserCount = accountCurrentLevel.MonthlyDepositCount
        });
    }

    private static Task<UsageLimitDto> PopulateOnUsPaymentLimitAsync(TierLevel tierLevel, AccountCurrentLevel accountCurrentLevel)
    {
        return Task.FromResult(new UsageLimitDto
        {
            MaxLimitEnabled = tierLevel.MaxOnUsPaymentLimitEnabled,
            DailyMaxAmount = tierLevel.DailyMaxOnUsPaymentAmount,
            DailyMaxCount = tierLevel.DailyMaxOnUsPaymentCount,
            DailyUserAmount = accountCurrentLevel.DailyOnUsPaymentAmount,
            DailyUserCount = accountCurrentLevel.DailyOnUsPaymentCount,
            MonthlyMaxAmount = tierLevel.MonthlyMaxOnUsPaymentAmount,
            MonthlyMaxCount = tierLevel.MonthlyMaxOnUsPaymentCount,
            MonthlyUserAmount = accountCurrentLevel.MonthlyOnUsPaymentAmount,
            MonthlyUserCount = accountCurrentLevel.MonthlyOnUsPaymentCount
        });
    }

    private static Task<UsageLimitDto> PopulateWithdrawLimitAsync(TierLevel tierLevel, AccountCurrentLevel accountCurrentLevel)
    {
        return Task.FromResult(new UsageLimitDto
        {
            MaxLimitEnabled = tierLevel.MaxWithdrawalLimitEnabled,
            DailyMaxAmount = tierLevel.DailyMaxWithdrawalAmount,
            DailyMaxCount = tierLevel.DailyMaxWithdrawalCount,
            DailyUserAmount = accountCurrentLevel.DailyWithdrawalAmount,
            DailyUserCount = accountCurrentLevel.DailyWithdrawalCount,
            MonthlyMaxAmount = tierLevel.MonthlyMaxWithdrawalAmount,
            MonthlyMaxCount = tierLevel.MonthlyMaxWithdrawalCount,
            MonthlyUserAmount = accountCurrentLevel.MonthlyWithdrawalAmount,
            MonthlyUserCount = accountCurrentLevel.MonthlyWithdrawalCount
        });
    }

    private static Task<IbanLimitDto> PopulateWithdrawToOwnIbanAsync(TierLevel tierLevel, AccountCurrentLevel accountCurrentLevel)
    {
        return Task.FromResult(new IbanLimitDto
        {
            MaxLimitEnabled = tierLevel.MaxOwnIbanWithdrawalLimitEnabled,
            DailyMaxCount = tierLevel.DailyMaxOwnIbanWithdrawalCount,
            MonthlyMaxCount = tierLevel.MonthlyMaxOwnIbanWithdrawalCount,
            DailyUserCount = accountCurrentLevel.DailyOwnIbanWithdrawalCount,
            MonthlyUserCount = accountCurrentLevel.MonthlyOwnIbanWithdrawalCount,
        });
    }

    private static Task<IbanLimitDto> PopulateWithdrawToOtherIbanAsync(TierLevel tierLevel, AccountCurrentLevel accountCurrentLevel)
    {
        return Task.FromResult(new IbanLimitDto
        {
            MaxLimitEnabled = tierLevel.MaxOtherIbanWithdrawalLimitEnabled,
            DailyMaxCount = tierLevel.DailyMaxOtherIbanWithdrawalCount,
            DailyMaxDistinctCount = tierLevel.DailyMaxDistinctOtherIbanWithdrawalCount,
            DailyMaxAmount = tierLevel.DailyMaxOtherIbanWithdrawalAmount,
            MonthlyMaxCount = tierLevel.MonthlyMaxOtherIbanWithdrawalCount,
            MonthlyMaxDistinctCount = tierLevel.MonthlyMaxDistinctOtherIbanWithdrawalCount,
            MonthlyMaxAmount = tierLevel.MonthlyMaxOtherIbanWithdrawalAmount,
            DailyUserCount = accountCurrentLevel.DailyOtherIbanWithdrawalCount,
            DailyUserDistinctCount = accountCurrentLevel.DailyDistinctOtherIbanWithdrawalCount,
            DailyUserAmount = accountCurrentLevel.DailyOtherIbanWithdrawalAmount,
            MonthlyUserCount = accountCurrentLevel.MonthlyOtherIbanWithdrawalCount,
            MonthlyUserDistinctCount = accountCurrentLevel.MonthlyDistinctOtherIbanWithdrawalCount,
            MonthlyUserAmount = accountCurrentLevel.MonthlyOtherIbanWithdrawalAmount
        });
    }

    private static Task<UsageLimitDto> PopulateInternalTransferLimitAsync(TierLevel tierLevel, AccountCurrentLevel userCurrentLevel)
    {
        return Task.FromResult(new UsageLimitDto
        {
            MaxLimitEnabled = tierLevel.MaxInternalTransferLimitEnabled,
            DailyMaxAmount = tierLevel.DailyMaxInternalTransferAmount,
            DailyMaxCount = tierLevel.DailyMaxInternalTransferCount,
            DailyUserAmount = userCurrentLevel.DailyInternalTransferAmount,
            DailyUserCount = userCurrentLevel.DailyInternalTransferCount,
            MonthlyMaxAmount = tierLevel.MonthlyMaxInternalTransferAmount,
            MonthlyMaxCount = tierLevel.MonthlyMaxInternalTransferCount,
            MonthlyUserAmount = userCurrentLevel.MonthlyInternalTransferAmount,
            MonthlyUserCount = userCurrentLevel.MonthlyInternalTransferCount
        });
    }

    private static Task<UsageLimitDto> PopulateInternationalTransferLimitAsync(TierLevel tierLevel, AccountCurrentLevel accountCurrentLevel)
    {
        return Task.FromResult(new UsageLimitDto
        {
            MaxLimitEnabled = tierLevel.MaxInternalTransferLimitEnabled,
            DailyMaxAmount = tierLevel.DailyMaxInternationalTransferAmount,
            DailyMaxCount = tierLevel.DailyMaxInternationalTransferCount,
            DailyUserAmount = accountCurrentLevel.DailyInternationalTransferAmount,
            DailyUserCount = accountCurrentLevel.DailyInternationalTransferCount,
            MonthlyMaxAmount = tierLevel.MonthlyMaxInternationalTransferAmount,
            MonthlyMaxCount = tierLevel.MonthlyMaxInternationalTransferCount,
            MonthlyUserAmount = accountCurrentLevel.MonthlyInternationalTransferAmount,
            MonthlyUserCount = accountCurrentLevel.MonthlyInternationalTransferCount
        });
    }

    private async Task<List<TierPermissionDto>> PopulateTierPermissionsAsync(TierLevel tierLevel)
    {
        return await _tierPermissionService.GetTierPermissionsQueryAsync(
            new GetTierPermissionsQuery
            {
                TierLevel = tierLevel.TierLevelType
            });
    }

    private async Task<List<TierLevelUpgradePathDto>> PopulateTierLevelUpgradePathsAsync(TierLevel tierLevel)
    {
        return await _tierLevelService.GetTierLevelUpgradePathsAsync(tierLevel.TierLevelType);
    }
}