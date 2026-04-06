using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierLevels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Services.Limit;

public class TierLevelService : ITierLevelService
{
    private readonly IGenericRepository<TierLevel> _tierLevelRepository;
    private readonly IMapper _mapper;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<AccountCurrentLevel> _accountCurrentLevelRepository;
    private readonly IGenericRepository<AccountCustomTier> _accountCustomTierRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IGenericRepository<TierLevelUpgradePath> _tierLevelUpgradePathRepository;
    private readonly IUserService _userService;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly ILogger<TierLevelService> _logger;
    private readonly IGenericRepository<AccountKycChange> _accountKycChangeRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IVirtualIbanService _virtualIbanService;

    public TierLevelService(
        IGenericRepository<TierLevel> tierLevelRepository,
        IMapper mapper,
        IContextProvider contextProvider,
        IGenericRepository<AccountCurrentLevel> accountCurrentLevelRepository,
        IGenericRepository<AccountCustomTier> accountCustomTierRepository,
        IGenericRepository<Account> accountRepository,
        IGenericRepository<TierLevelUpgradePath> tierLevelUpgradePathRepository,
        IUserService userService,
        IPushNotificationSender pushNotificationSender,
        ILogger<TierLevelService> logger,
        IGenericRepository<AccountKycChange> accountKycChangeRepository,
        IApplicationUserService applicationUserService,
        IVirtualIbanService virtualIbanService)
    {
        _tierLevelRepository = tierLevelRepository;
        _mapper = mapper;
        _contextProvider = contextProvider;
        _accountCurrentLevelRepository = accountCurrentLevelRepository;
        _accountCustomTierRepository = accountCustomTierRepository;
        _accountRepository = accountRepository;
        _tierLevelUpgradePathRepository = tierLevelUpgradePathRepository;
        _userService = userService;
        _pushNotificationSender = pushNotificationSender;
        _logger = logger;
        _accountKycChangeRepository = accountKycChangeRepository;
        _applicationUserService = applicationUserService;
        _virtualIbanService = virtualIbanService;
    }

    public async Task<List<TierLevelDto>> GetTierLevelsQueryAsync(GetTierLevelsQuery request)
    {
        var tierLevels = await _tierLevelRepository.GetAll(q => q.Currency)
            .OrderByDescending(q => q.TierLevelType)
            .ToListAsync();

        if (request.IncludeCustoms is false)
        {
            tierLevels = tierLevels.Where(x => x.TierLevelType != TierLevelType.Custom).ToList();
        }
        if (request.RecordStatus.HasValue)
        {
            tierLevels = tierLevels.Where(x => x.RecordStatus == request.RecordStatus).ToList();
        }
        if (!string.IsNullOrEmpty(request.CurrencyCode))
        {
            tierLevels = tierLevels.Where(x => x.CurrencyCode == request.CurrencyCode).ToList();
        }
        return _mapper.Map<List<TierLevelDto>>(tierLevels);
    }

    public async Task<AccountCurrentLevel> FindAccountCurrentLevel(Guid accountId, string currencyCode)
    {
        _ = Guid.TryParse(_contextProvider.CurrentContext.UserId, out Guid userId);

        var accountCurrentLevel = await _accountCurrentLevelRepository.GetAll()
            .SingleOrDefaultAsync(x => x.AccountId == accountId
                                       && x.RecordStatus == RecordStatus.Active
                                       && x.CurrencyCode == currencyCode);

        if (accountCurrentLevel is null)
        {
            return await PopulateInitialLevelAsync(currencyCode, accountId, userId);
        }

        var today = DateTime.Now;
        if (accountCurrentLevel.LevelDate.Date != today)
        {
            if (accountCurrentLevel.LevelDate.Year != today.Year
                || accountCurrentLevel.LevelDate.Month != today.Month)
            {
                accountCurrentLevel =
                    await PopulateInitialLevelAsync(accountCurrentLevel.CurrencyCode, accountId, userId);
            }
            else if (accountCurrentLevel.LevelDate.Month == today.Month
                     && accountCurrentLevel.LevelDate.Day != today.Day)
            {
                ResetDailyUsage(accountCurrentLevel);
                accountCurrentLevel.LevelDate = today;
            }
        }

        return accountCurrentLevel;
    }

    public Task<AccountCurrentLevel> PopulateInitialLevelAsync(string currencyCode, Guid accountId, Guid createdBy)
    {
        var level = new AccountCurrentLevel
        {
            CurrencyCode = currencyCode,
            LevelDate = DateTime.Now,
            DailyDepositAmount = 0,
            DailyDepositCount = 0,
            DailyWithdrawalAmount = 0,
            DailyWithdrawalCount = 0,
            DailyInternalTransferAmount = 0,
            DailyInternalTransferCount = 0,
            DailyInternationalTransferAmount = 0,
            DailyInternationalTransferCount = 0,
            DailyOwnIbanWithdrawalCount = 0,
            DailyOtherIbanWithdrawalCount = 0,
            DailyDistinctOtherIbanWithdrawalCount = 0,
            DailyOtherIbanWithdrawalAmount = 0,
            DailyOnUsPaymentAmount = 0,
            DailyOnUsPaymentCount = 0,
            MonthlyDepositAmount = 0,
            MonthlyDepositCount = 0,
            MonthlyWithdrawalAmount = 0,
            MonthlyWithdrawalCount = 0,
            MonthlyInternalTransferAmount = 0,
            MonthlyInternalTransferCount = 0,
            MonthlyInternationalTransferAmount = 0,
            MonthlyInternationalTransferCount = 0,
            MonthlyOwnIbanWithdrawalCount = 0,
            MonthlyOtherIbanWithdrawalCount = 0,
            MonthlyDistinctOtherIbanWithdrawalCount = 0,
            MonthlyOtherIbanWithdrawalAmount = 0,
            MonthlyOnUsPaymentAmount = 0,
            MonthlyOnUsPaymentCount = 0,
            AccountId = accountId,
            CreatedBy = createdBy.ToString()
        };

        return Task.FromResult(level);
    }

    public async Task<TierLevel> FindTierLevelAsync(Guid accountId)
    {
        // check custom limit defined for account
        var customTier = await _accountCustomTierRepository.GetAll()
            .Include(b => b.TierLevel)
            .Where(x => x.RecordStatus == RecordStatus.Active)
            .SingleOrDefaultAsync(x => x.AccountId == accountId);

        if (customTier is not null)
        {
            return customTier.TierLevel;
        }

        // find account tier level
        var account = await _accountRepository.GetByIdAsync(accountId);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), accountId);
        }

        var tierLevelType = GetTierLevelType(account.AccountKycLevel);

        var tierLevel = await _tierLevelRepository.GetAll()
            .Where(x => x.RecordStatus == RecordStatus.Active)
            .SingleOrDefaultAsync(x => x.TierLevelType == tierLevelType);

        if (tierLevel is null)
        {
            throw new NotFoundException(nameof(tierLevel));
        }

        return tierLevel;
    }

    public async Task<List<TierLevelUpgradePathDto>> GetTierLevelUpgradePathsAsync(TierLevelType tierLevelType)
    {
        var query = await _tierLevelUpgradePathRepository
            .GetAll().Where(up => up.RecordStatus == RecordStatus.Active && up.TierLevel == tierLevelType).ToListAsync();
        return _mapper.Map<List<TierLevelUpgradePathDto>>(query);
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

    public async Task CheckOrUpgradeAccountTierAsync(Account account, AccountTierValidation validationType)
    {
        var oldKycLevel = account.AccountKycLevel;
        try
        {
            account.AccountKycLevel = await GetNextKycLevelAsync(account.AccountKycLevel, validationType);
            account.KycChangeDate = DateTime.Now;

            var isUpgraded = oldKycLevel != account.AccountKycLevel;

            if (!isUpgraded)
            {
                return;
            }

            await _accountRepository.UpdateAsync(account);

            if ((await FindTierLevelAsync(account.Id)).TierLevelType != TierLevelType.Custom)
            {
                await SendP2PNotificationsAsync(account.Email, account.AccountKycLevel);
            }

            var checkKycForIbanAssignment = await _virtualIbanService.CheckKycLevelToIbanAssignmentAsync(account.AccountKycLevel);

            if (checkKycForIbanAssignment)
            {
                await _virtualIbanService.AssignToAccountAsync(account.Id);
            }

            await AddKycChangeLogAsync(account, validationType, oldKycLevel, true, null);
        }
        catch (Exception exception)
        {
            _logger.LogError($"KYC upgrade failed : {exception.Message}");
            await AddKycChangeLogAsync(account, validationType, oldKycLevel, false, exception.Message);
        }
    }

    private async Task AddKycChangeLogAsync(
        Account account, AccountTierValidation validationType,
        AccountKycLevel oldKycLevel, bool isUpgraded, string errorMessage)
    {
        await _accountKycChangeRepository.AddAsync(new AccountKycChange
        {
            AccountId = account.Id,
            ValidationType = validationType,
            OldKycLevel = oldKycLevel,
            NewKycLevel = account.AccountKycLevel,
            IsUpgraded = isUpgraded,
            ErrorMessage = errorMessage,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
        });
    }

    private async Task<AccountKycLevel> GetNextKycLevelAsync(AccountKycLevel currentLevel, AccountTierValidation validationType)
    {
        var currentTier = GetTierLevelType(currentLevel);

        var upgradePath = await _tierLevelUpgradePathRepository
            .GetAll()
            .Where(up => up.TierLevel == currentTier && up.ValidationType == validationType)
            .FirstOrDefaultAsync();

        if (upgradePath is null)
        {
            return currentLevel;
        }

        var checkTierLevelExists = await
            _tierLevelRepository
                .GetAll()
                .Where(t => t.TierLevelType == upgradePath.NextTier)
                .FirstOrDefaultAsync();

        return checkTierLevelExists is not null ? GetAccountKycLevel(upgradePath.NextTier) : currentLevel;

    }

    private async Task SendP2PNotificationsAsync(string email, AccountKycLevel newKycLevel)
    {
        var users = await _userService.GetAllUsersAsync(new GetUsersRequest() { Email = email });

        var notificationUsers = users.Items.Select(x =>
            {
                return new NotificationUserInfo
                {
                    UserId = x.Id,
                    FirstName = x.FirstName,
                    LastName = x.LastName
                };
            })
            .ToList();

        var userDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
        {
            UserIdList = notificationUsers.Select(x => x.UserId).ToList(),
        });

        var limits = await GetTierLevelsQueryAsync(new GetTierLevelsQuery());
        var limit = limits.FirstOrDefault(x => x.Name == newKycLevel.ToString());

        if (limit is not null)
        {
            var receiverNotificationRequest = new SendPushNotification
            {
                TemplateName = "KycUpgrade",
                TemplateParameters = new Dictionary<string, string>
                {
                    { "currentLimit", limit.MonthlyMaxDepositAmount.ToString("N2") },
                    { "currentDate", DateTime.Now.ToString("dd/MM/yyyy H:mm") }
                },
                Tokens = userDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
                UserList = notificationUsers
            };

            await _pushNotificationSender.SendPushNotificationAsync(receiverNotificationRequest);
        }
    }
    private static AccountKycLevel GetAccountKycLevel(TierLevelType tierLevelType)
    {
        return tierLevelType switch
        {
            TierLevelType.Tier0 => AccountKycLevel.NoneKyc,
            TierLevelType.Tier1 => AccountKycLevel.PreKyc,
            TierLevelType.Tier2 => AccountKycLevel.Kyc,
            TierLevelType.Tier3 => AccountKycLevel.Premium,
            TierLevelType.Tier4 => AccountKycLevel.PremiumPlus,
            TierLevelType.Tier5 => AccountKycLevel.ChildKyc,
            _ => throw new InvalidTierLevelException()
        };
    }
}