using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Application.Commons.Models.TodebModels;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.Emoney.Application.Features.Accounts;
using LinkPara.Emoney.Application.Features.Accounts.Commands.CreateAccount;
using LinkPara.Emoney.Application.Features.Accounts.Commands.PatchAccount;
using LinkPara.Emoney.Application.Features.Accounts.Commands.PatchAccountUser;
using LinkPara.Emoney.Application.Features.Limits.Queries.GetTierLevels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Domain.Events;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using MassTransit.Initializers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Transactions;

namespace LinkPara.Emoney.Infrastructure.Services;

public class AccountService : IAccountService
{
    private const string PatchUserGateway = "BackOffice";
    private const string MaskingString = "******";
    private readonly ILogger<AccountService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly IWalletNumberGenerator _walletNumberGenerator;
    private readonly IUserService _userService;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly ICustomerService _customerService;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly ITierLevelService _tierLevelService;
    private readonly IAccountingService _accountingService;
    private readonly IEmailSender _emailSender;
    private readonly IParameterService _parameterService;
    private readonly IBus _bus;
    private readonly HttpClient _client;
    private readonly IVaultClient _vaultClient;

    public AccountService(ILogger<AccountService> logger,
        IServiceScopeFactory scopeFactory,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IMapper mapper,
        IConfiguration configuration,
        IWalletNumberGenerator walletNumberGenerator,
        IUserService userService,
        IPushNotificationSender pushNotificationSender,
        ICustomerService customerService,
        IGenericRepository<Account> accountRepository,
        ITierLevelService tierLevelService,
        IAccountingService accountingService,
        IEmailSender emailSender,
        IParameterService parameterService,
        IGenericRepository<AccountUser> accountUserRepository,
        IBus bus,
        HttpClient client,
        IVaultClient vaultClient)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _mapper = mapper;
        _configuration = configuration;
        _walletNumberGenerator = walletNumberGenerator;
        _userService = userService;
        _pushNotificationSender = pushNotificationSender;
        _customerService = customerService;
        _accountRepository = accountRepository;
        _tierLevelService = tierLevelService;
        _accountingService = accountingService;
        _emailSender = emailSender;
        _parameterService = parameterService;
        _accountUserRepository = accountUserRepository;
        _bus = bus;
        _client = client;
        _vaultClient = vaultClient;
    }

    public async Task CreateAccountAsync(CreateAccountCommand request)
    {
        using var scope = _scopeFactory.CreateScope();
        Account account = null;
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var duplicateCheck = await dbContext.Account
                .AnyAsync(s =>
                        s.AccountType == request.AccountType &&
                        (s.AccountStatus == AccountStatus.Active || s.AccountStatus == AccountStatus.Suspended) &&
                        (s.Email == request.Email.ToLowerInvariant() ||
                        (s.PhoneCode == request.PhoneCode && s.PhoneNumber == request.PhoneNumber)));

        if (duplicateCheck)
        {
            if (request.ParentAccountId == Guid.Empty)
            {
                throw new DuplicateRecordException();
            }
        }

        var walletNumber = _walletNumberGenerator.Generate();

        var accountUser = new AccountUser
        {
            CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString(),
            UserId = request.IdentityUserId,
            Email = request.Email.ToLowerInvariant(),
            PhoneCode = request.PhoneCode,
            PhoneNumber = request.PhoneNumber,
            Firstname = request.Firstname,
            Lastname = request.Lastname
        };

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var newWallet = PopulateWallet(request.IdentityUserId, walletNumber);

            account = new Account
            {
                Id = request.AccountId ?? SequentialGuid.NewSequentialGuid(),
                IdentityNumber = request.IdentityNumber,
                Name = request.AccountType == AccountType.Individual ?
                      $"{request.Firstname} {request.Lastname}" :
                      request.CorporateTitle,
                Email = request.Email.ToLowerInvariant(),
                PhoneCode = request.PhoneCode,
                PhoneNumber = request.PhoneNumber,
                CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString(),
                AccountStatus = AccountStatus.Active,
                AccountType = request.AccountType,
                OpeningDate = DateTime.Now,
                CreateDate = DateTime.Now,
                RecordStatus = RecordStatus.Active,
                AccountKycLevel = request.AccountKycLevel,
                ParentAccountId = request.ParentAccountId,
                BirthDate = request.BirthDate,
                Profession = request.Profession,
                AccountUsers = new List<AccountUser>
                {
                    accountUser
                },
                Wallets = new List<Wallet> {
                    newWallet
                }
            };

            await dbContext.Account.AddAsync(account);

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();
        });

        if (request.AccountType == AccountType.Individual)
        {
            var customerResponse = await _customerService.CreateCustomerAsync(new CreateCustomerRequest
            {
                CustomerType = CustomerType.Individual,
                FirstName = accountUser.Firstname,
                LastName = accountUser.Lastname,
                UserId = accountUser.UserId,
                BirthDate = request.BirthDate,
                IdentityNumber = request.IdentityNumber,
                Profession = request.Profession,
                CreateCustomerProducts = PopulateCustomerProducts(accountUser.AccountId),
                CreateCustomerEmails = PopulateCustomerEmails(accountUser.Email),
                CreateCustomerPhones = PopulateCustomerPhones(accountUser.PhoneCode, accountUser.PhoneNumber)
            });

            if (customerResponse.CustomerId != Guid.Empty)
            {
                accountUser.Account.UpdateDate = DateTime.Now;
                accountUser.Account.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();
                accountUser.Account.CustomerId = customerResponse.CustomerId;
                accountUser.Account.CustomerNumber = customerResponse.CustomerNumber;
                await _accountRepository.UpdateAsync(accountUser.Account);
                if (account is not null)
                {
                    await SaveAccountingCustomerAsync(account, customerResponse.Customer, account.Wallets.FirstOrDefault());
                }
            }
            else
            {
                _logger.LogError($"Customer Could Not Be Created - AccountId : {accountUser.AccountId}");
            }
        }

        var details = new Dictionary<string, string>
            {
                {"AccountName", $"{request.Firstname} {request.Lastname}" },
                {"AccountType", request.AccountType.ToString() },
                {"Email", request.Email },
                {"Phone", string.Concat(request.PhoneCode, request.PhoneNumber) }
            };

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "SaveAccount",
                Resource = "Account",
                SourceApplication = "Emoney",
                UserId = !string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId)
                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                : Guid.Empty,
            }
        );
    }

    private async Task SaveAccountingCustomerAsync(Account account, CustomerDto customer, Wallet wallet)
    {
        if (customer is null)
        {
            _logger.LogError("Error On Save AccountingCustomer Customer is null");
            return;
        }

        await _accountingService.CreateCustomerAsync(account, wallet, customer);
    }

    public async Task<AccountDto> PatchAccountAsync(PatchAccountCommand command)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var dbCurrentAccount = await dbContext.Account
            .Include(s => s.Wallets)
            .Include(s => s.AccountUsers)
            .FirstOrDefaultAsync(s => s.Id == command.AccountId);

        if (dbCurrentAccount is null)
        {
            throw new NotFoundException(nameof(Account), command.AccountId!);
        }

        var accountUser = dbCurrentAccount.AccountUsers.FirstOrDefault();
        var tempAccountUser = new AccountUser
        {
            Email = accountUser.Email,
            PhoneCode = accountUser.PhoneCode,
            PhoneNumber = accountUser.PhoneNumber,
            UserId = accountUser.UserId,
            AccountId = dbCurrentAccount.Id
        };

        var requestAccountDto = _mapper.Map<PatchAccountDto>(dbCurrentAccount);

        command.PatchAccountDto.ApplyTo(requestAccountDto);

        if (dbCurrentAccount.AccountStatus == AccountStatus.Inactive &&
            requestAccountDto.AccountStatus == AccountStatus.Inactive)
        {
            throw new AlreadyDeactivatedException();
        }

        if (requestAccountDto.AccountKycLevel < dbCurrentAccount.AccountKycLevel)
        {
            if (dbCurrentAccount.ParentAccountId == Guid.Empty)
            {
                throw new InvalidKycLevelException();
            }
        }

        var changedEmail = string.Empty;
        if (requestAccountDto.Email != dbCurrentAccount.Email)
        {
            var accountEmailExists = await dbContext.Account
            .AnyAsync(s => s.Email == requestAccountDto.Email
            && (s.AccountStatus == AccountStatus.Active || s.AccountStatus == AccountStatus.Suspended));

            if (accountEmailExists)
            {
                if (dbCurrentAccount.ParentAccountId == Guid.Empty)
                {
                    throw new AlreadyInUseEmailException();
                }
            }

            changedEmail = requestAccountDto.Email;
        }
        var isPhoneNumberChanged = requestAccountDto.PhoneNumber != dbCurrentAccount.PhoneNumber;
        if (isPhoneNumberChanged)
        {
            var accountPhoneNumberExists = await dbContext.Account
            .AnyAsync(s => s.PhoneNumber == requestAccountDto.PhoneNumber
            && (s.AccountStatus == AccountStatus.Active || s.AccountStatus == AccountStatus.Suspended));

            if (accountPhoneNumberExists)
            {
                throw new AlreadyInUsePhoneNumberException();
            }
        }

        var isKycChanged = false;
        if (requestAccountDto.AccountKycLevel != dbCurrentAccount.AccountKycLevel)
        {
            dbCurrentAccount.KycChangeDate = DateTime.Now;
            isKycChanged = true;
            if (dbCurrentAccount.AccountKycLevel == AccountKycLevel.NoneKyc)
            {
                dbCurrentAccount.DeclarationStatus = DeclarationStatus.PendingToDeclare;
            }
        }
        var isProfessionChanged = requestAccountDto.Profession != dbCurrentAccount.Profession;

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var statusChanged = false;
            if (requestAccountDto.AccountStatus != dbCurrentAccount.AccountStatus)
            {
                statusChanged = true;
                switch (requestAccountDto.AccountStatus)
                {
                    case AccountStatus.Suspended:
                        dbCurrentAccount.RecordStatus = RecordStatus.Passive;
                        dbCurrentAccount.AccountStatus = AccountStatus.Suspended;
                        dbCurrentAccount.SuspendedDate = DateTime.Now;
                        break;

                    case AccountStatus.Inactive:
                        DateTime timestamp = DateTime.Now;
                        requestAccountDto.IdentityNumber = $"d_{timestamp.Ticks}_{dbCurrentAccount.IdentityNumber}";
                        dbCurrentAccount.RecordStatus = RecordStatus.Passive;
                        dbCurrentAccount.AccountStatus = AccountStatus.Inactive;
                        dbCurrentAccount.ClosingDate = DateTime.Now;
                        dbCurrentAccount.DeclarationStatus = DeclarationStatus.PendingToClose;
                        break;

                    case AccountStatus.Active:
                        dbCurrentAccount.RecordStatus = RecordStatus.Active;
                        dbCurrentAccount.AccountStatus = AccountStatus.Active;
                        dbCurrentAccount.ReopeningDate = DateTime.Now;
                        break;
                }

                UpdateAccountWalletsStatus(dbContext,
                    dbCurrentAccount.Wallets,
                    requestAccountDto.AccountStatus
                    );
            }

            UpdateAccountUser(dbContext, accountUser, requestAccountDto, statusChanged);

            dbCurrentAccount.UpdateDate = DateTime.Now;

            _mapper.Map(requestAccountDto, dbCurrentAccount);

            dbContext.Update(dbCurrentAccount);

            await dbContext.SaveChangesAsync();

            if (isKycChanged && dbCurrentAccount.AccountKycLevel == AccountKycLevel.Kyc)
            {
                await SendP2PNotificationsAsync(dbCurrentAccount.Email);
            }

            transactionScope.Complete();

            if (_contextProvider.CurrentContext.Gateway == PatchUserGateway)
            {
                await PatchUserAsync(tempAccountUser, requestAccountDto, dbCurrentAccount.AccountType, statusChanged);

                await UpdateCustomerAsync(tempAccountUser, requestAccountDto, statusChanged);
            }
            else
            {
                if (isProfessionChanged)
                {
                    await UpdateCustomerProfessionAsync(requestAccountDto);
                }
                if (isPhoneNumberChanged)
                {
                    await UpdateCustomerPhoneNumberAsync(requestAccountDto);
                }
            }
        });

        if (!string.IsNullOrEmpty(changedEmail))
        {
            await UpdateCustodyEmails(dbCurrentAccount, changedEmail);
        }

        return _mapper.Map<AccountDto>(dbCurrentAccount);
    }

    private async Task UpdateCustomerPhoneNumberAsync(PatchAccountDto request)
    {
        try
        {
            var customer = await _customerService.GetCustomerAsync(request.CustomerId);
            if (customer is not null)
            {
                var customerRequest = PopulateCustomerRequest(customer);
                customerRequest.CreateCustomerPhones = PopulateCustomerPhones(request.PhoneCode, request.PhoneNumber);
                await _customerService.CreateCustomerAsync(customerRequest);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateCustomerPhoneNumberError : {exception}");
        }
    }

    private async Task UpdateCustomerProfessionAsync(PatchAccountDto request)
    {
        try
        {
            var customer = await _customerService.GetCustomerAsync(request.CustomerId);
            if (customer is not null) 
            {
                var customerRequest = PopulateCustomerRequest(customer);
                customerRequest.Profession = request.Profession;
                await _customerService.CreateCustomerAsync(customerRequest);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateCustomerProfessionError : {exception}");
        }
    }

    private async Task UpdateCustodyEmails(Account account, string email)
    {
        var customer = await _customerService.GetCustomerAsync(account.CustomerId);
        var customerRequest = PopulateCustomerRequest(customer);
        customerRequest.CreateCustomerEmails = PopulateCustomerEmails(email);
        await _customerService.CreateCustomerAsync(customerRequest);

        var childAccounts = await _accountRepository.GetAll()
                                .Where(s => s.ParentAccountId == account.Id
                                    && s.RecordStatus == RecordStatus.Active).ToListAsync();

        foreach (var child in childAccounts)
        {
            var request = new PatchAccountCommand();
            request.AccountId = child.Id;
            request.PatchAccountDto = new JsonPatchDocument<PatchAccountDto>();
            request.PatchAccountDto.Replace(x => x.Email, email);
            await PatchAccountAsync(request);

            var childAccountUser = await _accountUserRepository.GetAll()
                .Where(x => x.AccountId == child.Id && x.RecordStatus == RecordStatus.Active)
                .FirstOrDefaultAsync();

            var userPatchRequest = new JsonPatchDocument<PatchUserRequest>();
            userPatchRequest.Operations.Add(new Operation<PatchUserRequest>
            {
                op = OperationType.Replace.ToString(),
                value = email,
                path = "email"
            });
            await _userService.PatchAsync(childAccountUser.UserId, userPatchRequest);
        }
    }

    private async Task SendP2PNotificationsAsync(string email)
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

        var limits = await _tierLevelService.GetTierLevelsQueryAsync(new GetTierLevelsQuery() { });

        var limit = limits.FirstOrDefault(x => x.Name == "Kyc");


        var receiverNotificationRequest = new SendPushNotification
        {
            TemplateName = "KycUpgrade",
            TemplateParameters = new Dictionary<string, string>
                {
                { "currentLimit", limit?.MonthlyMaxDepositAmount.ToString("N2") },
                { "currentDate", DateTime.Now.ToString("dd/MM/yyyy H:mm") }
            },
            Tokens = userDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = notificationUsers
        };

        await _pushNotificationSender.SendPushNotificationAsync(receiverNotificationRequest);
    }

    private Wallet PopulateWallet(Guid walletOwnerUserId, string walletNumber)
    {
        var walletSettings = new DefaultWalletSettings();
        _configuration.GetSection(nameof(DefaultWalletSettings)).Bind(walletSettings);

        var currencyCode = walletSettings.CurrencyCode.ToUpper();
        var walletName = walletSettings.FriendlyName;

        var wallet = new Wallet
        {
            CreateDate = DateTime.Now,
            CreatedBy = walletOwnerUserId.ToString(),
            LastActivityDate = DateTime.Now,
            RecordStatus = RecordStatus.Active,
            FriendlyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(walletName.ToLower()),
            CurrentBalanceCash = 0,
            CurrentBalanceCredit = 0,
            BlockedBalance = 0,
            CurrencyCode = currencyCode,
            WalletNumber = walletNumber,
            IsMainWallet = true,
            OpeningDate = DateTime.Now,
        };

        wallet.DomainEvents.Add(new WalletCreatedEvent(wallet));

        return wallet;
    }

    private void UpdateAccountUser(EmoneyDbContext dbContext, AccountUser accountUser,
        PatchAccountDto request, bool changeStatus)
    {
        if (changeStatus)
        {
            accountUser.RecordStatus =
                request.AccountStatus == AccountStatus.Active
                ? RecordStatus.Active
                : RecordStatus.Passive;
        }

        if ((accountUser.PhoneCode != request.PhoneCode) || accountUser.PhoneNumber != request.PhoneNumber)
        {
            accountUser.PhoneCode = request.PhoneCode;
            accountUser.PhoneNumber = request.PhoneNumber;
        }

        if (accountUser.Email.ToLowerInvariant() != request.Email.ToLowerInvariant())
        {
            accountUser.Email = request.Email.ToLowerInvariant();
        }

        accountUser.UpdateDate = DateTime.Now;
        accountUser.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();
        dbContext.Update(accountUser);
    }

    private void UpdateAccountWalletsStatus(EmoneyDbContext dbContext,
        List<Wallet> wallets,
        AccountStatus accountStatus
        )
    {
        var recordStatus = accountStatus == AccountStatus.Active
            ? RecordStatus.Active
            : RecordStatus.Passive;

        if (accountStatus == AccountStatus.Inactive &&
            wallets.Any(s => s.AvailableBalance > 0 && s.RecordStatus == RecordStatus.Active))
        {
            throw new WalletHasBalanceException();
        }

        foreach (var wallet in wallets)
        {
            wallet.RecordStatus = recordStatus;
            wallet.UpdateDate = DateTime.Now;
            wallet.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();

            if (recordStatus == RecordStatus.Active)
            {
                wallet.OpeningDate = DateTime.Now;
            }
            else
            {
                wallet.ClosingDate = DateTime.Now;
            }

            dbContext.Update(wallet);
        }
    }

    private async Task PatchUserAsync(AccountUser accountUser, PatchAccountDto request, AccountType accountType, bool statusChanged)
    {
        try
        {
            var userPatchRequest = new JsonPatchDocument<PatchUserRequest>();

            if (statusChanged)
            {
                userPatchRequest.Operations.Add(new Operation<PatchUserRequest>
                {
                    op = OperationType.Replace.ToString(),
                    value = GetUserStatus(request.AccountStatus),
                    path = "userStatus"
                });
            }

            if (accountType == AccountType.Individual)
            {
                if (accountUser.Email != request.Email)
                {
                    userPatchRequest.Operations.Add(new Operation<PatchUserRequest>
                    {
                        op = OperationType.Replace.ToString(),
                        value = request.Email,
                        path = "email"
                    });
                }


                if (accountUser.PhoneCode != request.PhoneCode)
                {
                    userPatchRequest.Operations.Add(new Operation<PatchUserRequest>
                    {
                        op = OperationType.Replace.ToString(),
                        value = request.PhoneCode,
                        path = "phoneCode"
                    });
                }

                if (accountUser.PhoneNumber != request.PhoneNumber)
                {
                    userPatchRequest.Operations.Add(new Operation<PatchUserRequest>
                    {
                        op = OperationType.Replace.ToString(),
                        value = request.PhoneNumber,
                        path = "phoneNumber"
                    });
                }
            }

            if (userPatchRequest.Operations.Count > 0)
            {
                await _userService.PatchAsync(accountUser.UserId, userPatchRequest);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateUserError : {exception}");
        }
    }

    public async Task UpdateCustomerAsync(AccountUser user, PatchAccountDto request, bool statusChanged)
    {
        try
        {
            var updateCustomer = false;

            var customer = await _customerService.GetCustomerAsync(request.CustomerId);

            if (customer is not null)
            {
                var customerRequest = PopulateCustomerRequest(customer);

                if ((user.PhoneCode != request.PhoneCode) || user.PhoneNumber != request.PhoneNumber)
                {
                    customerRequest.CreateCustomerPhones = PopulateCustomerPhones(request.PhoneCode, request.PhoneNumber);
                    updateCustomer = true;
                }

                if (user.Email.ToLowerInvariant() != request.Email.ToLowerInvariant())
                {
                    customerRequest.CreateCustomerEmails = PopulateCustomerEmails(request.Email);
                    updateCustomer = true;
                }

                if (statusChanged)
                {
                    var customerProduct = customerRequest.CreateCustomerProducts.FirstOrDefault(b => b.AccountId == user.AccountId);

                    NullControlHelper.CheckAndThrowIfNull(customerProduct, user.AccountId, _logger);

                    switch (request.AccountStatus)
                    {
                        case AccountStatus.Active:
                            {
                                customerProduct.ReopeningDate = DateTime.Now;
                                customerProduct.RecordStatus = RecordStatus.Active;
                                customerProduct.CustomerProductStatus = CustomerProductStatus.Active;
                                break;
                            }
                        case AccountStatus.Suspended:
                            {
                                customerProduct.SuspendedDate = DateTime.Now;
                                customerProduct.CustomerProductStatus = CustomerProductStatus.Suspended;
                                customerProduct.RecordStatus = RecordStatus.Passive;
                                break;
                            }
                        default:
                            {
                                customerProduct.ClosingDate = DateTime.Now;
                                customerProduct.CustomerProductStatus = CustomerProductStatus.Inactive;
                                customerProduct.RecordStatus = RecordStatus.Passive;
                                break;
                            }
                    }
                }

                if (updateCustomer || statusChanged)
                {
                    await _customerService.CreateCustomerAsync(customerRequest);
                }
            }
            else
            {
                _logger.LogError("Customer Not Found.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateCustomerError : {exception}");
        }
    }

    private List<CustomerProductDto> PopulateCustomerProducts(Guid accountId) =>
        new()
        {
            new CustomerProductDto
            {
                AccountId = accountId,
                OpeningDate = DateTime.Now,
                ProductType = ProductType.Emoney
            }
        };

    private List<CustomerPhoneDto> PopulateCustomerPhones(string code, string number) =>
        new()
        {
            new CustomerPhoneDto
            {
                PhoneCode = code,
                PhoneNumber = number,
                PhoneType = PhoneType.Individual,
                Primary = true
            }
        };

    private List<CustomerEmailDto> PopulateCustomerEmails(string email) =>
        new()
        {
            new CustomerEmailDto
            {
                Email = email,
                EmailType = EmailType.Individual,
                Primary = true
            }
        };

    private CreateCustomerRequest PopulateCustomerRequest(CustomerDto customer) =>
        new()
        {
            UserId = _contextProvider.CurrentContext.UserId != null
                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                : Guid.Empty,
            CustomerId = customer.Id,
            CommercialTitle = customer.CommercialTitle,
            TradeRegistrationNumber = customer.TradeRegistrationNumber,
            TaxAdministration = customer.TaxAdministration,
            TaxNumber = customer.TaxNumber,
            IdentityNumber = customer.IdentityNumber,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            DocumentType = customer.DocumentType,
            SerialNumber = customer.SerialNumber,
            BirthDate = customer.BirthDate,
            Profession = customer.Profession,
            NationCountryId = customer.NationCountryId,
            NationCountry = customer.NationCountry,
            CustomerType = customer.CustomerType,
            CreateCustomerAddresses = customer.CustomerAddresses,
            CreateCustomerProducts = customer.CustomerProducts
        };

    private static string GetUserStatus(AccountStatus status) =>
       status switch
       {
           AccountStatus.Active => UserStatus.Active.ToString(),
           AccountStatus.Inactive => UserStatus.Inactive.ToString(),
           AccountStatus.Suspended => UserStatus.Suspended.ToString(),
           _ => string.Empty,
       };

    public async Task PatchAccountUserAsync(PatchAccountUserCommand request)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var accountUser = dbContext.AccountUser
                                       .Where(x => x.Id == request.AccountUserId && x.AccountId == request.AccountId)
                                       .FirstOrDefault();

            if (accountUser is null)
            {
                throw new NotFoundException(nameof(AccountUser), request.AccountUserId);
            }

            var requestAccountUserDto = _mapper.Map<PatchAccountUserDto>(accountUser);

            request.PatchAccountUserDto.ApplyTo(requestAccountUserDto);

            if (requestAccountUserDto.Email != accountUser.Email)
            {
                var accountUserEmailExists = await dbContext.AccountUser
                                                        .AnyAsync(s => s.Email == requestAccountUserDto.Email &&
                                                                       s.RecordStatus == RecordStatus.Active);
                if (accountUserEmailExists)
                {
                    throw new AlreadyInUseEmailException();
                }
            }

            if (requestAccountUserDto.PhoneNumber != accountUser.PhoneNumber)
            {
                var accountUserPhoneNumberExists = await dbContext.AccountUser
                                                        .AnyAsync(s => s.PhoneNumber == requestAccountUserDto.PhoneNumber &&
                                                                       s.RecordStatus == RecordStatus.Active);
                if (accountUserPhoneNumberExists)
                {
                    throw new AlreadyInUsePhoneNumberException();
                }
            }

            accountUser.UpdateDate = DateTime.Now;

            _mapper.Map(requestAccountUserDto, accountUser);

            dbContext.Update(accountUser);

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();
        });

    }

    public async Task TransformCustodyAccountsAsync()
    {
        var custodyAccounts = await _accountRepository.GetAll()
                                   .Where(x => x.ParentAccountId != Guid.Empty && x.AccountStatus == AccountStatus.Active)
                                   .ToListAsync();

        var CustodyParameters = await _parameterService.GetParametersAsync("CustodyAccounts");
        foreach (var custodyAccount in custodyAccounts)
        {
            if (custodyAccount.BirthDate <= DateTime.Now.AddYears(-18))
            {
                var request = new PatchAccountCommand();
                request.AccountId = custodyAccount.Id;
                request.PatchAccountDto = new JsonPatchDocument<PatchAccountDto>();
                request.PatchAccountDto.Replace(x => x.ParentAccountId, Guid.Empty);
                request.PatchAccountDto.Replace(x => x.AccountKycLevel, AccountKycLevel.NoneKyc);
                await PatchAccountAsync(request);

                var parentAccount = await _accountRepository.GetAll()
                                   .Where(x => x.Id == custodyAccount.ParentAccountId && x.AccountStatus == AccountStatus.Active)
                                   .FirstOrDefaultAsync();

                string subject = CustodyParameters.FirstOrDefault(p => p.ParameterCode == "MailSubject")?.ParameterValue;
                string senderAddress = CustodyParameters.FirstOrDefault(p => p.ParameterCode == "MailSenderAddress")?.ParameterValue;
                subject = string.Concat(Environment.GetEnvironmentVariable("Tenant"), " ", subject);
                await _emailSender.SendEmailAsync(new()
                {
                    TemplateName = "TransformCustodyAccountsEmail",
                    ToEmail = parentAccount.Email,
                    DynamicTemplateData = new()
                        {
                            { "subject", subject },
                            { "name", custodyAccount.Name },
                            { "sendername", senderAddress },
                            { "parentName", parentAccount.Name}
                        }
                });
            }
        }

    }

    public async Task<AccountUser> GetCorporateAccountUserAsync(Guid userId)
    {
        var account = await _accountUserRepository
            .GetAll()
            .Include(s => s.Account)
            .Where(x => x.UserId == userId && x.Account.AccountType == AccountType.Corporate)
            .FirstOrDefaultAsync();

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), userId);
        }
        return account;
    }

    public async Task DeclareAccountsAsync()
    {
        var accountsToDeclare = await _accountRepository.GetAll()
                                                        .Where(x => x.DeclarationStatus ==  DeclarationStatus.PendingToDeclare
                                                                 && x.RecordStatus == RecordStatus.Active)
                                                        .OrderBy(x => x.CreateDate)
                                                        .Take(100)
                                                        .ToListAsync();

        if (accountsToDeclare.Any())
        {

            var identityListForDeclare = accountsToDeclare.Where(x => !string.IsNullOrWhiteSpace(x.IdentityNumber)
                                                              && x.IdentityNumber.Length == 11 && x.IdentityNumber.All(char.IsDigit))
                                                            .Select(x => x.IdentityNumber)
                                                            .Distinct()
                                                            .ToList();

            var responseForDeclareAccounts = await SendToEndpoint(identityListForDeclare, DeclarationStatus.PendingToDeclare);

            if (responseForDeclareAccounts != null)
            {
                if (responseForDeclareAccounts.SuccessfulRecords != null)
                {
                    var successfulRecords = responseForDeclareAccounts.SuccessfulRecords
                    .Select(x => x.IdentityNumber)
                    .ToList();

                    if (successfulRecords.Any())
                    {
                        var accountsToUpdate = accountsToDeclare
                            .Where(a => successfulRecords.Contains(a.IdentityNumber))
                            .ToList();

                        foreach (var account in accountsToUpdate)
                        {
                            account.DeclarationStatus = DeclarationStatus.Declared;
                            await _accountRepository.UpdateAsync(account);
                        }
                    }
                }

                if (responseForDeclareAccounts.UnsuccessfulRecords != null)
                {
                    var accountDictionary = accountsToDeclare.ToDictionary(x => x.IdentityNumber);

                    foreach (var record in responseForDeclareAccounts.UnsuccessfulRecords)
                    {
                        if (accountDictionary.TryGetValue(record.IdentityNumber, out var account))
                        {
                            if (record.Status == 200)
                            {
                                account.DeclarationStatus = DeclarationStatus.Declared;
                            }
                            else
                            {
                                account.DeclarationStatus = DeclarationStatus.Error;
                            }
                            await _accountRepository.UpdateAsync(account);
                        }
                    }
                }
            }
        }

        var accountsToClose = await _accountRepository.GetAll()
                                                        .Where(x => x.DeclarationStatus == DeclarationStatus.PendingToClose
                                                                 && x.RecordStatus == RecordStatus.Passive)
                                                        .OrderBy(x => x.CreateDate)
                                                        .Take(100)
                                                        .ToListAsync();

        if (accountsToClose.Any())
        {
            var identityListForClose = accountsToClose.Where(x => !string.IsNullOrWhiteSpace(x.IdentityNumber) && x.IdentityNumber.Length >= 11)
                                                        .Select(x => x.IdentityNumber[^11..])
                                                        .Distinct()
                                                        .ToList();

            var responseForCloseAccounts = await SendToEndpoint(identityListForClose, DeclarationStatus.PendingToClose);

            if (responseForCloseAccounts != null)
            {
                if (responseForCloseAccounts.SuccessfulRecords != null)
                {
                    var successfulRecords = responseForCloseAccounts.SuccessfulRecords
                    .Select(x => x.IdentityNumber)
                    .ToList();

                    if (successfulRecords.Any())
                    {                        
                        var successfulSet = successfulRecords.ToHashSet();
                        var accountsToUpdate = accountsToClose
                            .Where(a => !string.IsNullOrWhiteSpace(a.IdentityNumber)
                                        && a.IdentityNumber.Length >= 11
                                        && successfulSet.Contains(a.IdentityNumber[^11..]))
                            .ToList();

                        foreach (var account in accountsToUpdate)
                        {
                            account.DeclarationStatus = DeclarationStatus.Closed;
                            await _accountRepository.UpdateAsync(account);
                        }
                    }
                }

                if (responseForCloseAccounts.UnsuccessfulRecords != null)
                {                   
                    var accountDictionary = accountsToClose.Where(x => !string.IsNullOrWhiteSpace(x.IdentityNumber) && x.IdentityNumber.Length >= 11)
                                                            .ToDictionary(x => x.IdentityNumber[^11..], x => x );

                    foreach (var record in responseForCloseAccounts.UnsuccessfulRecords)
                    {
                        if (accountDictionary.TryGetValue(record.IdentityNumber, out var account))
                        {
                            account.DeclarationStatus = DeclarationStatus.Error;
                            await _accountRepository.UpdateAsync(account);
                        }
                    }
                }
            }
        }

        return;
    }
        

    private async Task<TodebDeclarationResponse> SendToEndpoint(List<string> identityList, DeclarationStatus status)
    {        
        var correlationId = Guid.NewGuid();
        string request = string.Empty;
        TodebDeclarationResponse responseModel = null;
        TodebDeclarationRequest requestModel = null;
        try
        {
            requestModel = new TodebDeclarationRequest
            {
                IdentityNumberList = identityList
            };

            var todebSettings = _vaultClient.GetSecretValue<TodebSettings>("EmoneySecrets", "TodebSettings");

            request = JsonSerializer.Serialize(requestModel);
            var username = todebSettings.Username; 
            var password = todebSettings.Password; 
            var url = todebSettings.EndPointUrl;

            var authValue = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{username}:{password}")
            );
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            await SendIntegrationLog(
                logName: "Todeb",
                logtype: status.ToString(),
                request: await MaskTodebRequestAsync(requestModel),
                response: string.Empty,
                error: string.Empty,
                correlationId: correlationId);

            HttpResponseMessage response = null;

            

            if (status == DeclarationStatus.PendingToDeclare)
            {
                response = await _client.PostAsync(url, new StringContent(request, Encoding.UTF8, "application/json"));
            }
            else if (status == DeclarationStatus.PendingToClose)
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Delete, url)
                {
                    Content = new StringContent(request, Encoding.UTF8, "application/json")
                };
                response = await _client.SendAsync(httpRequest);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Todeb Account Declaration Error : {response}" + responseContent);
                return null;
            }

            responseModel = JsonSerializer.Deserialize<TodebDeclarationResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await SendIntegrationLog(
            logName: "Todeb",
            logtype: status.ToString(),
            request: await MaskTodebRequestAsync(requestModel),
            response:await MaskTodebResponseAsync(responseModel),
            error: (!response.IsSuccessStatusCode) ? responseContent : string.Empty,
            correlationId: correlationId);

            return responseModel;
        }
        catch (Exception ex)
        {            
            await SendIntegrationLog(
                logName: "Todeb",
                logtype: status.ToString(),
                request: await MaskTodebRequestAsync(requestModel),
                response: string.Empty,
                error: ex.ToString(),
                correlationId: correlationId);

            _logger.LogError($"Todeb Account Declaration Error : {ex}", ex);

            return null;
        }
    }


    private async Task SendIntegrationLog(string logName, string logtype, string request, string response, Guid correlationId, string error)
    {
        try
        {
            var log = new IntegrationLog()
            {
                CorrelationId = correlationId.ToString(),
                Name = logName,
                Type = logtype,
                Date = DateTime.Now,
                Request = request,
                Response = response,    
                ErrorMessage = error,
                DataType = IntegrationLogDataType.Json
            };

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
            await endpoint.Send(log, cancellationToken.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Todeb Integration Log Error: {exception}");
        }

    }

    private static Task<string> MaskTodebRequestAsync(TodebDeclarationRequest request)
    {
        var maskedList = new List<string>(request.IdentityNumberList.Count);

        foreach (var tckn in request.IdentityNumberList)
        {
            string masked = tckn.Substring(0, 3) + MaskingString + tckn.Substring(9, 2);
            maskedList.Add(masked);
        }
                
        return Task.FromResult(JsonSerializer.Serialize(maskedList, new JsonSerializerOptions { WriteIndented = true }));
    }    

    private static Task<string> MaskTodebResponseAsync(TodebDeclarationResponse response)
    {
        var result = JsonSerializer.Deserialize<TodebDeclarationResponse>(JsonSerializer.Serialize(response));
        if (result.SuccessfulRecords != null)
        {
            foreach (var record in result.SuccessfulRecords)
            {
                record.IdentityNumber = record.IdentityNumber.Substring(0, 3) + MaskingString + record.IdentityNumber.Substring(9, 2);
            }
        }

        if (result.UnsuccessfulRecords != null)
        {
            foreach (var record in result.UnsuccessfulRecords)
            {
                record.IdentityNumber = record.IdentityNumber.Substring(0, 3) + MaskingString + record.IdentityNumber.Substring(9, 2);
            }
        }

        return Task.FromResult(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
    }
}


