using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.Emoney.Application.Features.Wallets;
using LinkPara.Emoney.Application.Features.Wallets.Commands.ConvertUserWalletsToIndividual;
using LinkPara.Emoney.Application.Features.Wallets.Commands.SaveWallet;
using LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateUserWallets;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateWallet;
using LinkPara.Emoney.Application.Features.Wallets.Commands.ValidateWallet;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetAccountWallets;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetUserWallets;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletBalanceDaily;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletBalances;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletDetails;
using LinkPara.Emoney.Application.Features.Wallets.Queries.GetWalletSummaries;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.BusinessParameter.Models;
using LinkPara.HttpProviders.Calendar;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;

namespace LinkPara.Emoney.Infrastructure.Services;

public class WalletService : IWalletService
{
    private readonly IGenericRepository<Wallet> _repository;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IGenericRepository<Currency> _currencyRepository;
    private readonly IWalletNumberGenerator _walletNumberGenerator;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly IParameterService _parameterService;
    private readonly IAccountingService _accountingService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<WalletService> _logger;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IEmailSender _emailSender;
    private readonly IUserService _userService;
    private readonly IPushNotificationSender _notificationSender;
    private readonly ITransferService _moneyTransferService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IGenericRepository<Transaction> _transactionRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ICalendarService _calendarService;
    public const string SuccessCode = ExceptionPrefix.Emoney + "000";
    public const string SuccessReasonCode = "Successful";
    public WalletService(
        IGenericRepository<Wallet> repository,
        IMapper mapper,
        IGenericRepository<AccountUser> accountUserRepository,
        IGenericRepository<Currency> currencyRepository,
        IWalletNumberGenerator walletNumberGenerator,
        IContextProvider contextProvider,
        IAuditLogService auditLogService,
        IParameterService parameterService,
        IAccountingService accountingService,
        ICustomerService customerService,
        ILogger<WalletService> logger,
        IGenericRepository<Account> accountRepository,
        IEmailSender emailSender,
        IUserService userService,
        IPushNotificationSender notificationSender,
        ITransferService moneyTransferService,
        IServiceScopeFactory scopeFactory,
        IGenericRepository<Transaction> transactionRepository,
        ICalendarService calendarService,
        IApplicationUserService applicationUserService)
    {
        _calendarService = calendarService;
        _repository = repository;
        _mapper = mapper;
        _accountUserRepository = accountUserRepository;
        _currencyRepository = currencyRepository;
        _walletNumberGenerator = walletNumberGenerator;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
        _parameterService = parameterService;
        _accountingService = accountingService;
        _customerService = customerService;
        _logger = logger;
        _accountRepository = accountRepository;
        _emailSender = emailSender;
        _userService = userService;
        _notificationSender = notificationSender;
        _moneyTransferService = moneyTransferService;
        _scopeFactory = scopeFactory;
        _transactionRepository = transactionRepository;
        _applicationUserService = applicationUserService;

    }

    public async Task<bool> IsBalanceSufficientAsync(string walletNumber, decimal amount, bool isCredit)
    {
        var query = _repository
             .GetAll()
             .Where(q => q.WalletNumber == walletNumber)
             .AsQueryable();

        if (isCredit)
        {
            query = query.Where(q => q.CurrentBalanceCredit >= amount);
        }
        else
        {
            query = query.Where(q => q.CurrentBalanceCash >= amount);
        }

        return await query.AnyAsync();
    }
    public async Task<List<WalletDto>> GetUserWalletsAsync(GetUserWalletsFilterQuery query)
    {
        var accountUser = await _accountUserRepository.GetAll()
                        .FirstOrDefaultAsync(s => s.UserId == query.UserId);

        if (accountUser is not null)
        {
            var wallets = await _repository.GetAll(s => s.Currency)
            .Where(s =>
                s.AccountId == accountUser.AccountId &&
                s.RecordStatus == RecordStatus.Active)
            .OrderBy(s => s.CreateDate)
            .ToListAsync();

            var walletDtos = _mapper.Map<List<WalletDto>>(wallets);

            bool p2PCreditBalanceUsable = await GetP2PCreditBalanceUsableAsync();
            walletDtos.ForEach(w =>
            {
                w.P2PCreditBalanceUsable = p2PCreditBalanceUsable;
            });

            return walletDtos;
        }

        return new List<WalletDto>();
    }
    public async Task<List<WalletDto>> GetAccountWalletsAsync(AccountWalletsQuery query)
    {
        var wallets = await _repository.GetAll(s => s.Currency)
            .Where(s =>
                s.AccountId == query.AccountId &&
                s.RecordStatus == RecordStatus.Active)
            .OrderBy(s => s.CreateDate)
            .ToListAsync();

        return _mapper.Map<List<WalletDto>>(wallets);
    }
    public async Task<WalletBalanceResponse> GetWalletBalancesAsync(GetWalletBalancesQuery query)
    {
        var response = new WalletBalanceResponse();
        var transactionDate = query.TransactionDate.Value;

        var walletsQuery = _repository
            .GetAll()
            .Where(w => w.CreateDate <= transactionDate);

        if (!string.IsNullOrEmpty(query.CurrencyCode))
            walletsQuery = walletsQuery.Where(w => w.CurrencyCode.Contains(query.CurrencyCode));

        if (!string.IsNullOrEmpty(query.AccountName))
            walletsQuery = walletsQuery.Where(w => w.Account.Name.ToLower().Contains(query.AccountName.ToLower()));

        if (query.RecordStatus is not null)
            walletsQuery = walletsQuery.Where(w => w.RecordStatus == query.RecordStatus);

        if (!string.IsNullOrEmpty(query.WalletNumber))
            walletsQuery = walletsQuery.Where(w => w.WalletNumber.Contains(query.WalletNumber));

        if (query.AccountId is not null)
            walletsQuery = walletsQuery.Where(w => w.AccountId == query.AccountId);

        if (!string.IsNullOrEmpty(query.BlockageStatus.ToString()))
        {
            if (query.BlockageStatus == WalletBlockageStatus.Blocked)
            {
                walletsQuery = walletsQuery.Where(w => w.IsBlocked == true);
            }

            if (query.BlockageStatus == WalletBlockageStatus.PartiallyBlocked)
            {
                walletsQuery = walletsQuery.Where(w => w.IsBlocked == false 
                                                   && (w.BlockedBalance != 0 || w.BlockedBalanceCredit != 0));
            }

            if (query.BlockageStatus == WalletBlockageStatus.Unblocked)
            {
                walletsQuery = walletsQuery.Where(w => w.IsBlocked == false
                                                   && (w.BlockedBalance == 0 || w.BlockedBalanceCredit == 0));
            }
        }

        var walletIds = await walletsQuery.Select(w => w.Id).ToListAsync();

        var lastTransactions = await _transactionRepository.GetAll()
            .Where(t => walletIds.Contains(t.WalletId) &&
            t.TransactionDate <= transactionDate)
            .GroupBy(t => t.WalletId)
            .Select(g => g
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .FirstOrDefault())
            .ToDictionaryAsync(t => t.WalletId);

        var walletBalanceList = await _repository.GetAll()
            .Where(w => walletIds.Contains(w.Id))
            .Include(w => w.Account)
                .ThenInclude(a => a.AccountUsers)
            .Select(w => new WalletBalanceDto
            {
                Id = w.Id,                
                CurrencyCode = w.CurrencyCode,
                WalletNumber = w.WalletNumber,
                FriendlyName = w.FriendlyName,
                AccountId = w.AccountId,
                Firstname = w.Account.AccountType == AccountType.Individual
                    ? w.Account.AccountUsers.FirstOrDefault().Firstname
                    : w.Account.Name,
                Lastname = w.Account.AccountType == AccountType.Individual
                    ? w.Account.AccountUsers.FirstOrDefault().Lastname
                    : string.Empty,
                RecordStatus = w.RecordStatus,
                Transactions = lastTransactions.ContainsKey(w.Id)
                    ? new List<Transaction> { lastTransactions[w.Id] }
                    : new List<Transaction>(),
                CurrentBalanceCredit = w.CurrentBalanceCredit,
                CurrentBalanceCash = w.CurrentBalanceCash,
                BlockedBalance = w.BlockedBalance,
                BlockedBalanceCredit = w.BlockedBalanceCredit,
                AvailableBalance = w.AvailableBalance,
                IsBlocked = w.IsBlocked
            })
            .PaginatedListAsync(query.Page, query.Size, query.OrderBy, query.SortBy);

        var totalBalance = lastTransactions.Values.Sum(t => t.CurrentBalance);

        response.WalletBalances = walletBalanceList;
        response.TotalBalance = totalBalance;

        return response;
    }
    public async Task<WalletDto> GetWalletDetailsAsync(GetWalletDetailsQuery query)
    {
        var accountUser = await _accountUserRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.UserId == query.UserId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), query.UserId);
        }

        var wallet = await _repository.GetAll(s => s.Currency)
            .SingleOrDefaultAsync(s =>
                s.Id == query.WalletId && s.AccountId == accountUser.AccountId);

        if (wallet is null)
        {
            throw new NotFoundException(nameof(Wallet), query.WalletId);
        }

        return _mapper.Map<WalletDto>(wallet);
    }
    public async Task<WalletSummaryDto> GetWalletSummaryAsync(GetWalletSummaryQuery query)
    {
        Wallet wallet = null;

        if (query.UserId != Guid.Empty)
        {
            var accountUser = await _accountUserRepository.GetAll()
                .FirstOrDefaultAsync(s => s.UserId == query.UserId);

            if (accountUser is null)
            {
                throw new NotFoundException(nameof(AccountUser), query.UserId);
            }

            wallet = await _repository.GetAll(s => s.Currency)
                .Include(s => s.Account)
                .SingleOrDefaultAsync(s => s.IsMainWallet &&
                                           s.AccountId == accountUser.AccountId &&
                                           s.RecordStatus == RecordStatus.Active);
        }
        else if (!string.IsNullOrEmpty(query.WalletNumber))
        {
            wallet = await _repository.GetAll(s => s.Currency)
                .Include(s => s.Account)
                .SingleOrDefaultAsync(s => s.WalletNumber == query.WalletNumber &&
                                           s.RecordStatus == RecordStatus.Active);
        }

        if (wallet is null)
        {
            throw new CustomizedWalletNotFoundException();
        }

        return new WalletSummaryDto
        {
            WalletNumber = wallet.WalletNumber,
            UserName = wallet.Account.Name,
            Balance = wallet.CurrentBalanceCash,
            CurrencySymbol = wallet.Currency.Symbol
        };
    }
    public async Task ConvertUserWalletsToIndividualAsync(ConvertUserWalletsToIndividualCommand command)
    {
        var accountUser = await _accountUserRepository.GetAll()
                            .FirstOrDefaultAsync(s => s.UserId == command.UserId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), command.UserId);
        }

        var wallets = await _repository.GetAll()
            .Where(s => s.AccountId == accountUser.AccountId)
            .ToListAsync();

        foreach (var wallet in wallets)
        {
            wallet.WalletType = WalletType.Individual;
            await _repository.UpdateAsync(wallet);
        }

        var user = await _userService.GetUserAsync(command.UserId);
        await _emailSender.SendEmailAsync(new SendEmail()
        {
            TemplateName = "MonthlyLimitObjectionApproved",
            ToEmail = user.Email
        });
    }
    public async Task SaveWalletAsync(SaveWalletCommand command)
    {
        var currency = await _currencyRepository.GetAll()
                    .FirstOrDefaultAsync(s =>
                    s.Code == command.CurrencyCode.ToUpper() &&
                        s.CurrencyType == command.CurrencyType);

        if (currency is null)
        {
            throw new NotFoundException(nameof(Currency), command.CurrencyCode);
        }

        var accountUser = await _accountUserRepository
            .GetAll()
            .Include(x => x.Account)
            .FirstOrDefaultAsync(s => s.UserId == command.UserId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), command.UserId);
        }

        var walletQuery = _repository.GetAll();

        var wallets = walletQuery
            .Where(x =>
                x.AccountId == accountUser.AccountId &&
                x.RecordStatus == RecordStatus.Active);

        var isDuplicate = wallets.ToList()
                .Any(x => x.FriendlyName.ToLower(CultureInfo.CurrentCulture).Trim() ==
                          command.FriendlyName.ToLower(CultureInfo.CurrentCulture).Trim());

        if (isDuplicate)
        {
            throw new DuplicateWalletException();
        }

        var accountSubWalletCount = walletQuery
            .Count(s => s.AccountId == accountUser.AccountId
                        && s.RecordStatus == RecordStatus.Active
                        && !s.IsMainWallet);

        var maxWalletLimit = await GetUserWalletLimit();

        if (accountSubWalletCount >= maxWalletLimit)
        {
            throw new MaxWalletLimitExceededException();
        }

        await CurrencyTypeControlAsync(currency, walletQuery, accountSubWalletCount);

        CheckMainWallet(command, walletQuery, accountSubWalletCount);

        var walletType = accountUser.Account.AccountType == AccountType.Corporate
            ? WalletType.Corporate
            : WalletType.Individual;

        var wallet = new Wallet
        {
            CreateDate = DateTime.Now,
            CreatedBy = command.UserId.ToString(),
            LastActivityDate = DateTime.Now,
            RecordStatus = RecordStatus.Active,
            AccountId = accountUser.AccountId,
            FriendlyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(command.FriendlyName.ToLower()),
            CurrentBalanceCash = 0,
            CurrentBalanceCredit = 0,
            BlockedBalance = 0,
            CurrencyCode = currency.Code,
            WalletNumber = _walletNumberGenerator.Generate(),
            IsMainWallet = command.IsMainWallet,
            OpeningDate = DateTime.Now,
            WalletType = walletType
        };

        if (string.IsNullOrWhiteSpace(_contextProvider.CurrentContext.UserId))
        {
            wallet.CreatedBy = command.UserId.ToString();
        }

        await _repository.AddAsync(wallet);

        var account = await _accountRepository.GetAll()
             .FirstOrDefaultAsync(s => s.Id == wallet.AccountId);

        await SaveAccountingCustomerAsync(account, account.CustomerId, wallet);

        await SaveWalletAuditLogAsync(true, command.UserId, new Dictionary<string, string>
        {
            {"WalletNumber", wallet.WalletNumber },
            {"FriendlyName", command.FriendlyName}
        });
    }
    public async Task UpdateUserWalletsAsync(Guid userId, UpdateUserWalletsCommand request)
    {
        var accountUser = await _accountUserRepository.GetAll()
                        .FirstOrDefaultAsync(s => s.UserId == request.UserId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser));
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var userWallets = await dbContext.Wallet
            .Where(q => q.AccountId == accountUser.AccountId)
            .ToListAsync();

        if (userWallets is null)
        {
            throw new NotFoundException(nameof(Wallet));
        }

        await PassiveRecordStatus(request, userWallets, dbContext);

        await ActiveRecordStatus(request, userWallets, dbContext);
    }
    public async Task UpdateWalletAsync(UpdateWalletCommand command, CancellationToken cancellationToken)
    {
        var accountUser = await _accountUserRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.UserId == command.UserId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser));
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var userWallets = await dbContext.Wallet
            .Where(s => s.AccountId == accountUser.AccountId)
            .ToListAsync();

        if (!userWallets.Any())
        {
            throw new NotFoundException(nameof(Wallet));
        }

        var wallet = userWallets.SingleOrDefault(q => q.Id == command.WalletId);

        if (wallet is null)
        {
            await UpdateWalletAuditLogAsync(false, command.UserId,
                new Dictionary<string, string>
                    { { "WalletNumber", null }, { "Exception", "NotFoundException" } });

            throw new NotFoundException(nameof(Wallet), command.WalletId);
        }

        if (command.FriendlyName != null)
        {
            var wallets = dbContext.Wallet
               .Where(x =>
                   x.Id == command.WalletId &&
                   x.AccountId == accountUser.AccountId &&
                   x.RecordStatus == RecordStatus.Active);

            var isDuplicate = wallets.ToList()
                .Any(x => x.FriendlyName.ToLower(CultureInfo.CurrentCulture).Trim() ==
                          command.FriendlyName.ToLower(CultureInfo.CurrentCulture).Trim());

            if (isDuplicate)
            {
                throw new DuplicateWalletException();
            }

            await UpdateFriendlyNameAsync(command, wallet, dbContext);
        }
        else if (wallet.RecordStatus != command.RecordStatus)
        {
            await UpdateWalletStatusAsync(command, cancellationToken, wallet, userWallets, accountUser, dbContext);
        }
    }
    public async Task<WalletPartnerDto> GetWalletDetailsPartnerAsync(GetWalletDetailsPartnerQuery query)
    {
        if (!string.IsNullOrEmpty(query.Msisdn))
        {
            var accountMain = await _accountRepository.GetAll()
            .FirstOrDefaultAsync(c => c.PhoneNumber == query.Msisdn);

            if (accountMain == null)
            {
                throw new NotFoundException(nameof(Account), query.Msisdn);
            }
            var walletForId = _repository.GetAll()
           .FirstOrDefault(s => s.AccountId == accountMain.Id);
            if (walletForId == null)
            {
                throw new NotFoundException(nameof(Wallet), accountMain.Id);
            }
            query.WalletId = walletForId.Id;
        }

        var walletPartner = new WalletPartnerDto();

        var accountUser = await _accountUserRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.UserId == query.UserId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), query.UserId);
        }
        walletPartner.AccountId = accountUser.AccountId;
        var wallet = await _repository.GetAll(s => s.Currency)
            .SingleOrDefaultAsync(s =>
                s.Id == query.WalletId && s.AccountId == accountUser.AccountId);

        if (wallet is null)
        {
            throw new NotFoundException(nameof(Wallet), query.WalletId);
        }

        walletPartner.WalletId = wallet.Id.ToString();
        walletPartner.WalletType = wallet.WalletType;
        walletPartner.IsBlocked = wallet.IsBlocked;
        walletPartner.RecordStatus = wallet.RecordStatus;
        walletPartner.LastStatusDate = wallet.UpdateDate;
        walletPartner.Balances = new BalancePartnerInfo(wallet.CurrencyCode, wallet.CurrentBalanceCredit, wallet.CurrentBalanceCash, wallet.BlockedBalance);

        var account = await _accountRepository.GetAll()
            .FirstOrDefaultAsync(c => c.Id == wallet.AccountId);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), query.WalletId);
        }
        walletPartner.SegmentType = account.AccountType;
        walletPartner.Msisdn = account.PhoneNumber;
        if (account.CustomerId != null)
        {
            var customer = await _customerService.GetCustomerAsync(account.CustomerId);
            if (customer is null)
            {
                throw new NotFoundException(nameof(Account), query.WalletId);
            }
            walletPartner.Customer = new CustomerPartnerInfo(customer.FirstName, customer.LastName, customer.IdentityNumber, customer.Profession, customer.BirthDate);
        }
        return walletPartner;
    }
    private async Task SaveAccountingCustomerAsync(Account account, Guid customerId, Wallet wallet)
    {
        var customer = await _customerService.GetCustomerAsync(customerId);

        if (customer is null)
        {
            _logger.LogError("Error On Save AccountingCustomer Customer is null");
            return;
        }

        await _accountingService.CreateCustomerAsync(account, wallet, customer);
    }
    private static async Task CurrencyTypeControlAsync(Currency currency, IQueryable<Wallet> walletQuery, int userSubWalletCount)
    {
        if (currency.CurrencyType == CurrencyType.Crypto && userSubWalletCount > 0)
        {
            var hasCryptoWallet = await walletQuery
                .AnyAsync(s =>
                    s.CurrencyCode == currency.Code &&
                    s.Currency.CurrencyType == CurrencyType.Crypto &&
                    s.RecordStatus == RecordStatus.Active);

            if (hasCryptoWallet)
            {
                throw new MaxWalletLimitExceededException();
            }
        }
    }
    private static void CheckMainWallet(SaveWalletCommand request, IQueryable<Wallet> walletQuery, int userSubWalletCount)
    {
        if (userSubWalletCount > 0 && request.IsMainWallet)
        {
            var hasMainWallet = walletQuery
                 .Any(s =>
                     s.IsMainWallet &&
                     s.RecordStatus == RecordStatus.Active);

            if (hasMainWallet)
            {
                throw new MainWalletAlreadyExistsException();
            }
        }
    }
    private async Task SaveWalletAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> deatils)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "SaveWallet",
                Resource = "Wallet",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }
    private async Task<int> GetUserWalletLimit()
    {
        var parameter = await _parameterService
            .GetParameterAsync("MaxSubWalletLimit", "Limit");

        if (parameter == null)
        {
            throw new NotFoundException(nameof(ParameterTemplateValueDto));
        }

        return !String.IsNullOrEmpty(parameter.ParameterValue)
            ? Convert.ToInt32(parameter.ParameterValue)
            : 0;
    }
    private async Task UpdateFriendlyNameAsync(UpdateWalletCommand request, Wallet wallet, EmoneyDbContext dbContext)
    {
        dbContext.Wallet.Attach(wallet);

        wallet.FriendlyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(request.FriendlyName.ToLower());
        wallet.LastActivityDate = DateTime.Now;

        await dbContext.SaveChangesAsync();

        await UpdateWalletAuditLogAsync(true, request.UserId, new Dictionary<string, string>
        {
            { "WalletNumber", wallet.WalletNumber },
            { "FriendlyName", request.FriendlyName }
        });
    }
    private async Task UpdateWalletStatusAsync(UpdateWalletCommand request, CancellationToken cancellationToken, Wallet wallet,
       List<Wallet> userWallets, AccountUser user, EmoneyDbContext dbContext)
    {
        if (wallet.IsMainWallet && request.RecordStatus == RecordStatus.Passive)
        {
            await UpdateWalletAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                { "WalletNumber", wallet.WalletNumber },
                { "Exception", "CanNotCloseMainWalletException" }
            });

            throw new CanNotCloseMainWalletException();
        }

        if (wallet.RecordStatus == RecordStatus.Passive && !wallet.IsBlocked)
        {
            await UpdateWalletAuditLogAsync(false, request.UserId, new Dictionary<string, string>
                { { "WalletNumber", wallet.WalletNumber }, { "Exception", "AlreadyDeactivatedException" } });

            throw new AlreadyDeactivatedException(nameof(Wallet));
        }
        await UpdateWalletStatus(request, wallet, userWallets, user, cancellationToken, dbContext);
    }
    private async Task UpdateWalletAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> details)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "UpdateWallet",
                Resource = "Wallet",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }
    private async Task UpdateWalletStatus(UpdateWalletCommand request, Wallet wallet, List<Wallet> userWallets, AccountUser user,
        CancellationToken cancellationToken, EmoneyDbContext dbContext)
    {

        switch (request.RecordStatus)
        {
            case RecordStatus.Passive:

                if (!request.IsBlocked)
                {
                    if (wallet.AvailableBalance > 0)
                    {
                        var userMainWallet = userWallets.SingleOrDefault(q => q.IsMainWallet);

                        if (userMainWallet is null)
                        {
                            throw new NotFoundException(nameof(Wallet.IsMainWallet));
                        }

                        var transferRequest = new TransferCommand
                        {
                            Amount = wallet.AvailableBalance,
                            ReceiverWalletNumber = userMainWallet.WalletNumber,
                            SenderWalletNumber = wallet.WalletNumber,
                            UserId = request.UserId.ToString(),
                            Description = "Alt Hesap Transferi"
                        };

                        var response = await _moneyTransferService.TransferAsync(transferRequest, cancellationToken);

                        if (!response.Success)
                        {
                            throw new InvalidOperationException(response.ErrorMessage);
                        }

                        wallet = await dbContext.Wallet.FirstOrDefaultAsync(s => s.Id == wallet.Id);
                    }
                }

                dbContext.Wallet.Attach(wallet);

                if (request.IsBlocked)
                {
                    wallet.IsBlocked = true;
                }

                wallet.RecordStatus = RecordStatus.Passive;
                wallet.ClosingDate = DateTime.Now;

                await dbContext.SaveChangesAsync();

                await SendPushNotificationAsync(user, wallet, userWallets.SingleOrDefault(x => x.IsMainWallet));

                break;

            case RecordStatus.Active:

                wallet.IsBlocked = false;
                wallet.RecordStatus = RecordStatus.Active;
                wallet.OpeningDate = DateTime.Now;

                await dbContext.SaveChangesAsync();

                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(request.RecordStatus));
        }

        await UpdateWalletAuditLogAsync(true, request.UserId, new Dictionary<string, string>
        {
            { "WalletNumber", wallet.WalletNumber },
            { "wallet.WalletStatus", request.RecordStatus.ToString() },
            { "wallet.IsBlocked.", request.IsBlocked.ToString() }
        });
    }
    private async Task SendPushNotificationAsync(AccountUser user, Wallet subWallet, Wallet mainWallet)
    {
        var receiverUserIdList = new List<NotificationUserInfo>()
        {
            new NotificationUserInfo
            {
               UserId = user.UserId,
               FirstName = user.Firstname,
               LastName = user.Lastname,
            }
        };

        var receiverUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
        {
            UserIdList = receiverUserIdList.Select(x => x.UserId).ToList(),
        });
        var receiverNotificationRequest = new SendPushNotification
        {
            TemplateName = "SubWalletClosing",
            TemplateParameters = new Dictionary<string, string>
            {
                { "subWalletName", subWallet.FriendlyName },
                { "mainWalletName", mainWallet.FriendlyName },
                { "currentDate", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
            },
            Tokens = receiverUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = receiverUserIdList
        };

        await _notificationSender.SendPushNotificationAsync(receiverNotificationRequest);
    }
    private async Task PassiveRecordStatus(UpdateUserWalletsCommand request, List<Wallet> userWallets, EmoneyDbContext dbContext)
    {
        if (request.RecordStatus == RecordStatus.Passive)
        {
            if (userWallets.Any(userWallet =>
                    userWallet.AvailableBalance > 0 && userWallet.RecordStatus == RecordStatus.Active))
            {
                throw new WalletHasBalanceException();
            }

            foreach (var wallet in userWallets)
            {
                dbContext.Wallet.Attach(wallet);

                wallet.RecordStatus = RecordStatus.Passive;
                wallet.ClosingDate = DateTime.Now;

                await dbContext.SaveChangesAsync();

                await SaveUpdateUserWalletsAuditLogAsync(wallet.WalletNumber, "InactiveWallet");
            }
        }
    }
    private async Task ActiveRecordStatus(UpdateUserWalletsCommand request, List<Wallet> userWallets, EmoneyDbContext dbContext)
    {
        if (request.RecordStatus == RecordStatus.Active)
        {
            if (request.IsBlockage)
            {
                foreach (var wallet in userWallets.Where(s => !s.IsBlocked))
                {
                    dbContext.Wallet.Attach(wallet);

                    wallet.IsBlocked = true;

                    await dbContext.SaveChangesAsync();

                    await SaveUpdateUserWalletsAuditLogAsync(wallet.WalletNumber, "BlockageWallet");
                }
            }
            else
            {
                foreach (var wallet in userWallets.Where(s => s.IsBlocked))
                {
                    dbContext.Wallet.Attach(wallet);

                    wallet.IsBlocked = false;
                    wallet.OpeningDate = DateTime.Now;

                    await dbContext.SaveChangesAsync();

                    await SaveUpdateUserWalletsAuditLogAsync(wallet.WalletNumber, "RemoveBlockage");
                }
            }
        }
    }
    private async Task SaveUpdateUserWalletsAuditLogAsync(string walletNumber, string status)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateWalletStatus",
                SourceApplication = "Emoney",
                Resource = "Wallet",
                Details = new Dictionary<string, string>
                {
                        { "WalletNumber", walletNumber },
                        { "Status", status },
                }
            });
    }
    private async Task<bool> GetP2PCreditBalanceUsableAsync()
    {
        bool p2PCreditBalanceUsable = true;

        try
        {
            var parameter = await _parameterService.GetParameterAsync("EmoneyTransferParameters", "P2PCreditBalanceUsable");
            p2PCreditBalanceUsable = Convert.ToBoolean(parameter.ParameterValue);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Parameter : {exception}");
        }

        return p2PCreditBalanceUsable;
    }
    public async Task SyncWalletBalanceDailyAsync()
    {
        var jobTime = DateTime.Now;
        var hourAgo = jobTime.AddHours(-1);
        var hourAfter = jobTime.AddHours(1);
        var hourAgoIsHoliday = await _calendarService.IsHolidayAsync(hourAgo, "TUR");
        var hourAfterIsHoliday = await _calendarService.IsHolidayAsync(hourAfter, "TUR");
        if (
        (!hourAgoIsHoliday && !hourAfterIsHoliday && jobTime.Hour > 12) ||
        (!hourAgoIsHoliday && hourAfterIsHoliday && jobTime.Hour < 12))
        {
            var response = await GetWalletBalanceDaily(jobTime);
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
            await dbContext.WalletBalanceDaily.AddAsync(response);
            await dbContext.SaveChangesAsync();
        }
        return;
    }
    private async Task<WalletBalanceDaily> GetWalletBalanceDaily(DateTime jobDate)
    {
        var response = new WalletBalanceDaily();
        var transactionDate = jobDate;

        var walletsQuery = _repository
            .GetAll()
            .Where(w => w.CreateDate <= transactionDate);
        var walletIds = await walletsQuery.Select(w => w.Id).ToListAsync();

        var lastTransactions = await _transactionRepository.GetAll()
            .Where(t => walletIds.Contains(t.WalletId) &&
            t.TransactionDate <= transactionDate)
            .GroupBy(t => t.WalletId)
            .Select(g => g
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.Id)
                .FirstOrDefault())
            .ToDictionaryAsync(t => t.WalletId);

        var totalBalance = lastTransactions.Values.Sum(t => t.CurrentBalance);
        response.DailyBalance = totalBalance;
        response.Currency = lastTransactions.FirstOrDefault().Value.CurrencyCode;
        response.JobDate = jobDate;
        response.CreatedBy = _applicationUserService.ApplicationUserId.ToString();

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
            var yesterday = jobDate.Date.AddDays(-1);
            var yesterdayBalance = await dbContext.WalletBalanceDaily
                .AsNoTracking()
                .Where(w => w.JobDate.Date <= yesterday)
                .OrderByDescending(w => w.JobDate)
                .FirstOrDefaultAsync();

            if (yesterdayBalance != null)
            {
                response.Difference = yesterdayBalance.DailyBalance - totalBalance;
            }
        }
        return response;
    }
    public async Task<WalletBalanceDailyResponse> GetWalletBalanceDailyAsync(GetWalletBalancesDailyQuery query)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
        var response = new WalletBalanceDailyResponse();

        if (!query.TransactionDate.HasValue)
        {
            var result = await dbContext.WalletBalanceDaily
                .AsNoTracking()
                .OrderByDescending(w => w.JobDate)
                .Take(10)
                .PaginatedListAsync(query.Page, query.Size, query.OrderBy, query.SortBy);

            response.WalletBalances = result;
            return response;
        }
        else
        {
            var result = await dbContext.WalletBalanceDaily
                .AsNoTracking()
                .Where(w => w.JobDate.Date == query.TransactionDate.Value.Date)
                .PaginatedListAsync(query.Page, query.Size, query.OrderBy, query.SortBy);

            response.WalletBalances = result;
            return response;
        }
    }
    public async Task<List<MoneyTransferPaymentType>> GetMoneyTransferPaymentTypeAsync()
    {
        var parameterList = await _parameterService.GetParametersAsync("MoneyTransferPaymentType");
        var moneyTransferPaymentTypeList = new List<MoneyTransferPaymentType>();
        foreach (var parameter in parameterList)
        {
            var parameterTemplateValues = await _parameterService.GetAllParameterTemplateValuesAsync("MoneyTransferPaymentType", parameter.ParameterCode);
            var paymentType = new MoneyTransferPaymentType();

            foreach (var item in parameterTemplateValues)
            {
                switch (item.TemplateCode)
                {
                    case "Code":
                        paymentType.Code = int.Parse(item.TemplateValue);
                        break;

                    case "Name":
                        paymentType.Name = item.TemplateValue;
                        break;

                    case "Permission":
                        paymentType.Permission = bool.Parse(item.TemplateValue);
                        break;

                    case "DescriptionLength":
                        paymentType.DescriptionLength = int.Parse(item.TemplateValue);
                        break;
                }
            }

            moneyTransferPaymentTypeList.Add(paymentType);
        }
        return moneyTransferPaymentTypeList.OrderBy(x => x.Code).ToList();
    }
    public async Task<ValidateWalletResponse> ValidateWalletAsync(ValidateWalletCommand command)
    {
        Wallet wallet;
        try
        {
            wallet = await GetWalletAsync(command.WalletNumber);

            var currencyCodeNumber = await GetCurrencyCodeAsync(wallet.CurrencyCode);
            ValidateStatus(wallet, command.CurrencyCode, currencyCodeNumber);
            return new ValidateWalletResponse
            {
                ResponseCode = SuccessCode,
                ResponseReasonCode = SuccessReasonCode
            };
        }
        catch (Exception exception)
        {
            if (exception is ApiException apiException)
            {
                return new ValidateWalletResponse
                {
                    ResponseCode = apiException.Code,
                    ResponseReasonCode = apiException.Message
                };
            }
            throw;
        }
    }
    private async Task<Wallet> GetWalletAsync(string walletNumber)
    {
        var wallet = await _repository
            .GetAll()
            .SingleOrDefaultAsync(x => x.WalletNumber == walletNumber);
        if (wallet is null)
        {
            throw new CustomizedWalletNotFoundException();
        }
        return wallet;
    }
    private async Task<string> GetCurrencyCodeAsync(string currencyCode)
    {
        var parameterTemplateValue = await _parameterService
            .GetAllParameterTemplateValuesAsync("Currencies", currencyCode);
        return parameterTemplateValue?.FirstOrDefault(b => b.TemplateCode == "Number")?.TemplateValue;
    }
    private static void ValidateStatus(Wallet wallet, string requestCurrencyCode, string walletCurrencyCode)
    {
        if (requestCurrencyCode != walletCurrencyCode)
        {
            throw new CurrencyCodeMismatchException();
        }
        if (wallet.RecordStatus == RecordStatus.Passive)
        {
            throw new InvalidWalletStatusException();
        }
        if (wallet.IsBlocked)
        {
            throw new WalletBlockedException();
        }
    }
}
