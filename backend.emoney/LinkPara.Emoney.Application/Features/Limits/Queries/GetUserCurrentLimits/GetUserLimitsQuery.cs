using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetUserCurrentLimits;

public class GetUserLimitsQuery : IRequest<UserLimitDto>
{
    public Guid UserId { get; set; }
    public string CurrencyCode { get; set; }
}

public class GetUserLimitsQueryHandler : IRequestHandler<GetUserLimitsQuery, UserLimitDto>
{
    private readonly IGenericRepository<TierLevel> _tierLevelRepository;
    private readonly IGenericRepository<AccountCustomTier> _accountCustomTierLevelRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly ITierLevelService _tierLevelService;
    private readonly ITierPermissionService _tierPermissionService;

    public GetUserLimitsQueryHandler(IGenericRepository<TierLevel> tierLevelRepository,
        IGenericRepository<Wallet> walletRepository,
        IGenericRepository<AccountCustomTier> accountCustomTierLevelRepository,
        IGenericRepository<AccountUser> accountUserRepository,
        ITierLevelService tierLevelService,
        ITierPermissionService tierPermissionService)
    {
        _tierLevelRepository = tierLevelRepository;
        _walletRepository = walletRepository;
        _accountCustomTierLevelRepository = accountCustomTierLevelRepository;
        _accountUserRepository = accountUserRepository;
        _tierLevelService = tierLevelService;
        _tierPermissionService = tierPermissionService;
    }

    public async Task<UserLimitDto> Handle(GetUserLimitsQuery request,
        CancellationToken cancellationToken)
    {
        var accountUser = await _accountUserRepository.GetAll()
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s =>
                s.UserId == request.UserId &&
                s.RecordStatus == RecordStatus.Active);

        if (accountUser == null)
        {
            throw new NotFoundException(nameof(AccountUser), request.UserId);
        }

        var account = accountUser.Account;

        if (account == null)
        {
            throw new NotFoundException(nameof(Account), request.UserId);
        }

        TierLevel tierLevel;

        var customTierLevel = await _accountCustomTierLevelRepository.GetAll()
           .Include(x => x.TierLevel)
           .ThenInclude(x => x.Currency)
           .FirstOrDefaultAsync(x => x.AccountId == account.Id &&
           x.RecordStatus == RecordStatus.Active, cancellationToken);

        tierLevel = customTierLevel?.TierLevel;

        if (tierLevel is null)
        {
            tierLevel = await _tierLevelRepository.GetAll(q => q.Currency)
                .SingleOrDefaultAsync(f => f.TierLevelType == GetTierLevelType(account.AccountKycLevel),
                    cancellationToken);

            if (tierLevel is null)
            {
                throw new NotFoundException(nameof(tierLevel), account.AccountKycLevel);
            }
        }

        var accountCurrentLevel = await _tierLevelService.FindAccountCurrentLevel(account.Id, request.CurrencyCode);

        if (accountCurrentLevel is null)
        {
            throw new NotFoundException(nameof(AccountCurrentLevel), account.Id);
        }

        var currentUsageSummary = new UserLimitDto
        {
            CurrencySymbol = tierLevel.Currency.Symbol,
            UserId = accountUser.UserId,
            TierLevelType = tierLevel.TierLevelType,
            Name = tierLevel.Name,
            IndividualWallet = await PopulateBalanceLimitAsync(tierLevel, account.Id, WalletType.Individual),
            Deposit = await PopulateDepositLimitAsync(tierLevel, accountCurrentLevel),
            Withdraw = await PopulateWithdrawLimitAsync(tierLevel, accountCurrentLevel),
            WithdrawToOwnIban = await PopulateWithdrawToOwnIbanAsync(tierLevel, accountCurrentLevel),
            WithdrawToOtherIban = await PopulateWithdrawToOtherIbanAsync(tierLevel, accountCurrentLevel),
            InternalTransfer = await PopulateInternalTransferLimitAsync(tierLevel, accountCurrentLevel),
            InternationalTransfer = await PopulateInternationalTransferLimitAsync(tierLevel, accountCurrentLevel),
            TierPermissions = await PopulateTierPermissionsAsync(tierLevel),
            TierLevelUpgradePaths = await PopulateTierLevelUpgradePathsAsync(tierLevel)
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
            new GetTierPermissionsQuery.GetTierPermissionsQuery
            {
                TierLevel = tierLevel.TierLevelType
            });
    }
    
    private async Task<List<TierLevelUpgradePathDto>> PopulateTierLevelUpgradePathsAsync(TierLevel tierLevel)
    {
        return await _tierLevelService.GetTierLevelUpgradePathsAsync(tierLevel.TierLevelType);
    }
}