using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.Emoney.Application.Features.CompanyPools.Commands.ApproveCompanyPool;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkPara.Emoney.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Transactions;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.Emoney.Domain.Events;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeleteUser;
using Microsoft.AspNetCore.JsonPatch;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateUser;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateUser;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.AddUser;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.ActivateCorporateAccount;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.DeactivateCorporateAccount;
using LinkPara.Emoney.Application.Features.CorporateWallets.Commands.UpdateCorporateAccount;
using LinkPara.Emoney.Application.Commons.Exceptions;
using DocumentType = LinkPara.HttpProviders.Identity.Models.Enums.DocumentType;

namespace LinkPara.Emoney.Infrastructure.Services;

public class CorporateWalletService : ICorporateWalletService
{
    private readonly IGenericRepository<CompanyPool> _companyPoolRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ICustomerService _customerService;
    private readonly IUserService _userService;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger<CorporateWalletService> _logger;
    private readonly IAccountingService _accountingService;
    private readonly IBus _bus;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly IWalletNumberGenerator _walletNumberGenerator;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IAuditLogService _auditLogService;

    private readonly Guid AdminRoleId = new Guid("fa4a86a5-7149-477b-9e07-b6d472094d33");

    public CorporateWalletService(IGenericRepository<CompanyPool> repository,
        IApplicationUserService applicationUserService,
        ICustomerService customerService,
        IGenericRepository<Account> accountRepository,
        IUserService userService,
        IContextProvider contextProvider,
        ILogger<CorporateWalletService> logger,
        IAccountingService accountingService,
        IBus bus,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        IWalletNumberGenerator walletNumberGenerator,
        IGenericRepository<AccountUser> accountUserRepository,
        IAuditLogService auditLogService)
    {
        _companyPoolRepository = repository;
        _applicationUserService = applicationUserService;
        _customerService = customerService;
        _accountRepository = accountRepository;
        _userService = userService;
        _contextProvider = contextProvider;
        _logger = logger;
        _accountingService = accountingService;
        _bus = bus;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _walletNumberGenerator = walletNumberGenerator;
        _accountUserRepository = accountUserRepository;
        _auditLogService = auditLogService;
    }

    public async Task ActionCompanyPoolAsync(ApproveCompanyPoolCommand request)
    {
        var companyPool = await _companyPoolRepository.GetByIdAsync(request.CompanyPoolId);

        if (companyPool is null)
        {
            throw new NotFoundException(nameof(companyPool), request.CompanyPoolId);
        }

        if (request.UserId.ToString() == companyPool.CreatedBy)
        {
            throw new CreaterAndApproverAreSameException();
        }

        companyPool.ActionUser = request.UserId;
        companyPool.ActionDate = DateTime.Now;

        switch (request.IsApprove)
        {
            case true:
                await CreateNewAccountAsync(companyPool);
                companyPool.CompanyPoolStatus = CompanyPoolStatus.Approved;
                break;
            default:
                companyPool.RejectReason = request.RejectReason;
                companyPool.CompanyPoolStatus = CompanyPoolStatus.Rejected;
                break;
        }

        await _companyPoolRepository.UpdateAsync(companyPool);
    }

    private async Task CreateNewAccountAsync(CompanyPool companyPool)
    {
        UserCreateResponse userCreateResponse = null;
        Account account = null;
        try
        {
            var userCreateRequest = await PrepareUserCreateRequestAsync(companyPool);

            userCreateResponse = await _userService.CreateUserAsync(userCreateRequest);

            account = await CreateAccountAsync(companyPool, userCreateResponse.UserId);
            companyPool.AccountId = account.Id;
        }
        catch (Exception exception)
        {

            _logger.LogError($"Account Creating Error : {exception}");

            if (userCreateResponse is not null)
            {

                var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Identity.DeleteUser"));
                await endpoint.Send(new DeleteUser
                {
                    UserId = Guid.Parse(userCreateResponse.UserId.ToString())
                }, tokenSource.Token);
            }

            if (exception is ValidationException)
            {
                throw;
            }
            else if (exception is ApiException)
            {
                throw new CustomApiException(((ApiException)exception).Code, exception.Message);
            }

            throw;
        }
    }

    private async Task<Account> CreateAccountAsync(CompanyPool companyPool, Guid userId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            Account account = null;
            var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

            var duplicateCheck = await dbContext.Account
                    .AnyAsync(s =>
                            s.AccountType == AccountType.Corporate &&
                            (s.AccountStatus == AccountStatus.Active || s.AccountStatus == AccountStatus.Suspended) &&
                            (s.Email == companyPool.Email.ToLowerInvariant() ||
                            (s.PhoneCode == companyPool.PhoneCode && s.PhoneNumber == companyPool.PhoneNumber)));

            if (duplicateCheck)
            {
                throw new DuplicateRecordException();
            }

            var walletNumber = _walletNumberGenerator.Generate();

            var accountUser = new AccountUser
            {
                CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString(),
                UserId = userId,
                Email = companyPool.AuthorizedPersonEmail.ToLowerInvariant(),
                PhoneCode = companyPool.AuthorizedPersonCompanyPhoneCode,
                PhoneNumber = companyPool.AuthorizedPersonCompanyPhoneNumber,
                Firstname = companyPool.AuthorizedPersonName,
                Lastname = companyPool.AuthorizedPersonSurname
            };

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var newWallet = PopulateWallet(userId, walletNumber);

                account = new Account
                {
                    Id = companyPool.Id,
                    Name = companyPool.Title,
                    Email = companyPool.Email.ToLowerInvariant(),
                    PhoneCode = companyPool.PhoneCode,
                    PhoneNumber = companyPool.PhoneNumber,
                    CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString(),
                    AccountStatus = AccountStatus.Active,
                    AccountType = AccountType.Corporate,
                    OpeningDate = DateTime.Now,
                    CreateDate = DateTime.Now,
                    RecordStatus = RecordStatus.Active,
                    AccountKycLevel = AccountKycLevel.CorporateKyc,
                    AccountUsers = new List<AccountUser>
                {
                    accountUser
                },
                    Wallets = new List<Wallet> {
                    newWallet
                }
                };

                var customerRequest = PrepareCustomerRequest(companyPool);

                var product = new CustomerProductDto
                {
                    OpeningDate = DateTime.Now,
                    AccountId = companyPool.Id,
                    ProductType = ProductType.CorporateWallet,
                    CustomerProductStatus = CustomerProductStatus.Active
                };

                customerRequest.CreateCustomerProducts.Add(product);

                customerRequest.CustomerType = companyPool.CompanyType switch
                {
                    CompanyType.Individual => CustomerType.Individual,
                    CompanyType.Corporate => CustomerType.Corporate,
                    _ => customerRequest.CustomerType
                };

                var customerResponse = await _customerService.CreateCustomerAsync(customerRequest);

                if (customerResponse.CustomerId != Guid.Empty)
                {
                    account.UpdateDate = DateTime.Now;
                    account.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();
                    account.CustomerId = customerResponse.CustomerId;
                    account.CustomerNumber = customerResponse.CustomerNumber;

                    if (account is not null)
                    {
                        await SaveAccountingCustomerAsync(account, customerResponse.Customer, account.Wallets.FirstOrDefault());
                    }
                }
                else
                {
                    _logger.LogError($"Customer Could Not Be Created - AccountId : {account.Id}");
                }

                await dbContext.Account.AddAsync(account);

                await dbContext.SaveChangesAsync();

                transactionScope.Complete();
            });
            return account;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Exception on CreateAccountAsync: \n {exception}");

            if (exception is ValidationException)
            {
                throw;
            }
            else if (exception is ApiException)
            {
                throw new CustomApiException(((ApiException)exception).Code, exception.Message);
            }
            throw;
        }
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
            WalletType = WalletType.Corporate
        };

        wallet.DomainEvents.Add(new WalletCreatedEvent(wallet));

        return wallet;
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

    private async Task<CreateUserRequest> PrepareUserCreateRequestAsync(CompanyPool request)
    {
        var userName = await GetUserNameHelper.GetUserNameAsync(UserTypePrefix.CorporateWallet, request.AuthorizedPersonCompanyPhoneCode, request.AuthorizedPersonCompanyPhoneNumber);

        await CheckDuplicateUserAsync(userName);

        return new CreateUserRequest
        {
            BirthDate = request.AuthorizedPersonBirthDate,
            Email = request.AuthorizedPersonEmail,
            FirstName = request.AuthorizedPersonName,
            LastName = request.AuthorizedPersonSurname,
            PhoneCode = request.AuthorizedPersonCompanyPhoneCode,
            PhoneNumber = request.AuthorizedPersonCompanyPhoneNumber,
            UserType = UserType.CorporateWallet,
            UserName = userName,
            Roles = new List<Guid> { AdminRoleId }
        };
    }

    private async Task CheckDuplicateUserAsync(string userName)
    {
        var userList = await _userService.GetAllUsersAsync(new GetUsersRequest { UserName = userName });
        var activeUserStatus = new List<UserStatus> { UserStatus.Suspended, UserStatus.Active };
        if (userList.TotalCount > 0 && userList.Items.Any(x => activeUserStatus.Contains(x.UserStatus)))
        {
            throw new DuplicateRecordException(nameof(UserDto), userName);
        }
    }

    private CreateCustomerRequest PrepareCustomerRequest(CompanyPool companyPool)
    {

        var address = new CustomerAddressDto();
        var productList = new List<CustomerProductDto>();
        var addressList = new List<CustomerAddressDto>();
        var customerRequest = new CreateCustomerRequest();

        if (companyPool.CompanyType == CompanyType.Individual)
        {
            customerRequest.FirstName = companyPool.AuthorizedPersonName;
            customerRequest.LastName = companyPool.AuthorizedPersonSurname;
            customerRequest.BirthDate = companyPool.AuthorizedPersonBirthDate;
            customerRequest.NationCountry = "TR";
            customerRequest.NationCountryId = "TR";
            customerRequest.IdentityNumber = companyPool.AuthorizedPersonIdentityNumber;
            customerRequest.DocumentType = DocumentType.Identity;
        }
        customerRequest.CommercialTitle = companyPool.Title;
        customerRequest.TaxNumber = companyPool.TaxNumber;
        customerRequest.TaxAdministration = companyPool.TaxAdministration;
        customerRequest.TradeRegistrationNumber = companyPool.MersisNumber;
        customerRequest.UserId = _applicationUserService.ApplicationUserId;

        address.DistrictId = companyPool.District;
        address.District = companyPool.DistrictName;
        address.CountryId = companyPool.Country;
        address.Country = companyPool.CountryName;
        address.City = companyPool.CityName;
        address.CityId = companyPool.City;
        address.Address = companyPool.Address;
        address.PostalCode = companyPool.PostalCode;
        address.AddressType = AddressType.Company;
        addressList.Add(address);

        customerRequest.CreateCustomerAddresses = addressList;
        customerRequest.CreateCustomerProducts = productList;
        customerRequest.CreateCustomerPhones = CreatePhonesDto(companyPool);
        customerRequest.CreateCustomerEmails = CreateEmailsDto(companyPool);

        return customerRequest;
    }

    private List<CustomerPhoneDto> CreatePhonesDto(CompanyPool companyPool)
    {
        var phoneList = new List<CustomerPhoneDto>();
        var customerIndividualPhone = new CustomerPhoneDto()
        {
            PhoneCode = companyPool.AuthorizedPersonCompanyPhoneCode,
            PhoneNumber = companyPool.AuthorizedPersonCompanyPhoneNumber,
            Primary = true,
            PhoneType = PhoneType.Individual,
        };
        var customerCompanyPhone = new CustomerPhoneDto()
        {
            PhoneCode = companyPool.PhoneCode,
            PhoneNumber = companyPool.PhoneNumber,
            Primary = false,
            PhoneType = PhoneType.Company,
        };

        phoneList.Add(customerIndividualPhone);
        phoneList.Add(customerCompanyPhone);

        return phoneList;
    }
    private List<CustomerEmailDto> CreateEmailsDto(CompanyPool companyPool)
    {
        var emailList = new List<CustomerEmailDto>();
        var customerCompanyEmail = new CustomerEmailDto()
        {
            Email = companyPool.Email,
            EmailType = EmailType.Company,
            Primary = true,

        };
        var customerIndividualEmail = new CustomerEmailDto()
        {
            Email = companyPool.AuthorizedPersonEmail,
            EmailType = EmailType.Individual,
            Primary = false,

        };
        emailList.Add(customerIndividualEmail);
        emailList.Add(customerCompanyEmail);

        return emailList;
    }

    public async Task AddCorporateWalletUserAsync(AddCorporateWalletUserCommand request)
    {
        try
        {
            var account = await _accountRepository.GetAll()
            .AnyAsync(s =>
                s.Id == request.AccountId &&
                s.RecordStatus == RecordStatus.Active);

            if (!account)
            {
                throw new NotFoundException(nameof(Account), request.AccountId);
            }

            var userCreateRequest = await PrepareUserCreateRequestAsync(request);
            var userCreateResponse = await _userService.CreateUserAsync(userCreateRequest);

            var accountUser = new AccountUser
            {
                CreatedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString(),
                UserId = userCreateResponse.UserId,
                Email = request.Email.ToLowerInvariant(),
                PhoneCode = request.PhoneCode,
                PhoneNumber = request.PhoneNumber,
                Firstname = request.Firstname,
                Lastname = request.Lastname,
                AccountId = request.AccountId
            };
            await _accountUserRepository.AddAsync(accountUser);

            var details = new Dictionary<string, string>
        {
            {"Firstname", request.Firstname},
            {"Lastname", request.Lastname },
            {"Email", request.Email },
            {"Phone", string.Concat(request.PhoneCode, request.PhoneNumber) },
            {"UserId", userCreateResponse.UserId.ToString() },
            {"AccountId", request.AccountId.ToString() },
        };

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    Details = details,
                    LogDate = DateTime.Now,
                    Operation = "SaveAccountUser",
                    Resource = "AccountUser",
                    SourceApplication = "Emoney",
                    UserId = !string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId)
                    ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                    : Guid.Empty,
                }
            );
        }
        catch (Exception exception)
        {
            if (exception is ApiException)
            {
                throw new CustomApiException(((ApiException)exception).Code, exception.Message);
            }
            throw;
        }
    }

    private async Task<CreateUserRequest> PrepareUserCreateRequestAsync(AddCorporateWalletUserCommand request)
    {
        var userName = await GetUserNameHelper.GetUserNameAsync(UserTypePrefix.CorporateWallet, request.PhoneCode, request.PhoneNumber);

        await CheckDuplicateUserAsync(userName);

        return new CreateUserRequest
        {
            BirthDate = request.BirthDate,
            Email = request.Email,
            FirstName = request.Firstname,
            LastName = request.Lastname,
            PhoneCode = request.PhoneCode,
            PhoneNumber = request.PhoneNumber,
            UserType = UserType.CorporateWallet,
            UserName = userName,
            Roles = request.Roles
        };
    }

    public async Task DeactivateCorporateWalletUserAsync(DeactivateCorporateWalletUserCommand request)
    {
        var account = await _accountRepository.GetAll()
          .AnyAsync(s =>
              s.Id == request.AccountId &&
              s.RecordStatus == RecordStatus.Active);

        if (!account)
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        var accountUser = await _accountUserRepository
            .GetAll()
            .Where(x => x.Id == request.AccountUserId
                    && x.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync();

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), request.AccountUserId);
        }

        var userPatchRequest = new JsonPatchDocument<PatchUserRequest>();
        userPatchRequest.Replace(x => x.UserStatus, UserStatus.Suspended);
        await _userService.PatchAsync(accountUser.UserId, userPatchRequest);

        await _accountUserRepository.DeleteAsync(accountUser);

        var details = new Dictionary<string, string>
        {
            {"Firstname", accountUser.Firstname},
            {"Lastname", accountUser.Lastname },
            {"Email", accountUser.Email },
            {"Phone", string.Concat(accountUser.PhoneCode, accountUser.PhoneNumber) },
            {"UserId", accountUser.UserId.ToString() },
            {"AccountId", request.AccountId.ToString() },
        };

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "DeactivateAccountUser",
                Resource = "AccountUser",
                SourceApplication = "Emoney",
                UserId = !string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId)
                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                : Guid.Empty,
            }
        );
    }

    public async Task UpdateCorporateWalletUserAsync(UpdateCorporateWalletUserCommand request)
    {
        var account = await _accountRepository.GetAll()
          .AnyAsync(s =>
              s.Id == request.AccountId &&
              s.RecordStatus == RecordStatus.Active);

        if (!account)
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        var accountUser = await _accountUserRepository
            .GetAll()
            .Where(x => x.Id == request.AccountUserId
                    && x.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync();

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), request.AccountUserId);
        }

        var userPatchRequest = new JsonPatchDocument<PatchUserRequest>();

        if (accountUser.PhoneCode != request.PhoneCode)
        {
            userPatchRequest.Replace(x => x.PhoneCode, request.PhoneCode);
            accountUser.PhoneCode = request.PhoneCode;
        }

        if (accountUser.PhoneNumber != request.PhoneNumber)
        {
            userPatchRequest.Replace(x => x.PhoneNumber, request.PhoneNumber);
            accountUser.PhoneNumber = request.PhoneNumber;
        }

        if (accountUser.Email != request.Email)
        {
            userPatchRequest.Replace(x => x.Email, request.Email);
            accountUser.Email = request.Email;
        }

        userPatchRequest.Replace(x => x.Roles, request.Roles);

        await _userService.PatchAsync(accountUser.UserId, userPatchRequest);
        await _accountUserRepository.UpdateAsync(accountUser);

    }

    public async Task ActivateCorporateWalletUserAsync(ActivateCorporateWalletUserCommand request)
    {
        var account = await _accountRepository.GetAll()
          .AnyAsync(s =>
              s.Id == request.AccountId &&
              s.RecordStatus == RecordStatus.Active);

        if (!account)
        {
            throw new NotFoundException(nameof(Account), request.AccountId);
        }

        var accountUser = await _accountUserRepository
            .GetAll()
            .Where(x => x.Id == request.AccountUserId
                    && x.RecordStatus == RecordStatus.Passive)
            .FirstOrDefaultAsync();

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), request.AccountUserId);
        }

        var userPatchRequest = new JsonPatchDocument<PatchUserRequest>();
        userPatchRequest.Replace(x => x.UserStatus, UserStatus.Active);
        await _userService.PatchAsync(accountUser.UserId, userPatchRequest);

        accountUser.RecordStatus = RecordStatus.Active;
        await _accountUserRepository.UpdateAsync(accountUser);

        var details = new Dictionary<string, string>
        {
            {"Firstname", accountUser.Firstname},
            {"Lastname", accountUser.Lastname },
            {"Email", accountUser.Email },
            {"Phone", string.Concat(accountUser.PhoneCode, accountUser.PhoneNumber) },
            {"UserId", accountUser.UserId.ToString() },
            {"AccountId", request.AccountId.ToString() },
        };

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "ActivateAccountUser",
                Resource = "AccountUser",
                SourceApplication = "Emoney",
                UserId = !string.IsNullOrEmpty(_contextProvider.CurrentContext.UserId)
                ? Guid.Parse(_contextProvider.CurrentContext.UserId)
                : Guid.Empty,
            }
        );
    }

    public async Task ActivateCorporateAccountAsync(ActivateCorporateAccountCommand request)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
        var accountId = request.Id;

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            var dbCurrentAccount = await dbContext.Account
                .Include(s => s.Wallets)
                .Include(s => s.AccountUsers)
                .Include(s => s.CompanyPool)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == accountId);
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            dbCurrentAccount.RecordStatus = RecordStatus.Active;
            dbCurrentAccount.AccountStatus = AccountStatus.Active;
            dbCurrentAccount.OpeningDate = DateTime.Now;

            UpdateAccountWalletsStatus(dbContext,
                dbCurrentAccount.Wallets,
                dbCurrentAccount.AccountStatus
                );


            UpdateAccountUsers(dbContext, dbCurrentAccount.AccountUsers, dbCurrentAccount.AccountStatus);

            dbCurrentAccount.UpdateDate = DateTime.Now;

            dbContext.Update(dbCurrentAccount);

            await dbContext.SaveChangesAsync();


            transactionScope.Complete();

            var companyPool = dbCurrentAccount.CompanyPool;

            var request = new UpdateCorporateAccountCommand
            {
                Email = dbCurrentAccount.Email,
                PhoneCode = dbCurrentAccount.PhoneCode,
                PhoneNumber = dbCurrentAccount.PhoneNumber,
                Address = companyPool.Address,
                City = companyPool.City,
                CityName = companyPool.CityName,
                Country = companyPool.Country,
                CountryName = companyPool.CountryName,
                District = companyPool.District,
                DistrictName = companyPool.DistrictName,
                PostalCode = companyPool.PostalCode
            };

            await UpdateCustomerAsync(dbCurrentAccount, request, true);
        });

    }

    public async Task UpdateCustomerAsync(Account account, UpdateCorporateAccountCommand updateRequest, bool statusChanged)
    {
        try
        {
            var updateCustomer = false;

            var customer = await _customerService.GetCustomerAsync(account.CustomerId);

            if (customer is not null)
            {
                var customerRequest = PopulateCustomerRequest(customer);

                if ((updateRequest.PhoneCode != account.PhoneCode) || updateRequest.PhoneNumber != account.PhoneNumber)
                {
                    customerRequest.CreateCustomerPhones = PopulateCustomerPhones(account.PhoneCode, account.PhoneNumber);
                    updateCustomer = true;
                }

                if (updateRequest.Email.ToLowerInvariant() != account.Email.ToLowerInvariant())
                {
                    customerRequest.CreateCustomerEmails = PopulateCustomerEmails(account.Email);
                    updateCustomer = true;
                }

                var address = customer.CustomerAddresses.FirstOrDefault(x => x.Primary);


                if (address is not null && (updateRequest.Country != address.CountryId ||
                    updateRequest.CountryName.ToLowerInvariant() != address.Country.ToLowerInvariant() ||
                    updateRequest.City != address.CityId ||
                    updateRequest.CityName.ToLowerInvariant() != address.City.ToLowerInvariant() ||
                    updateRequest.District != address.DistrictId ||
                    updateRequest.DistrictName.ToLowerInvariant() != address.District.ToLowerInvariant() ||
                    updateRequest.PostalCode != address.PostalCode ||
                    updateRequest.Address.ToLowerInvariant() != address.Address.ToLowerInvariant()
                    ))
                {
                    customerRequest.CreateCustomerAddresses = PopulateCustomerAddresses(updateRequest);
                    updateCustomer = true;
                }

                if (statusChanged)
                {
                    var customerProduct = customerRequest.CreateCustomerProducts.FirstOrDefault(b => b.AccountId == account.Id);

                    NullControlHelper.CheckAndThrowIfNull(customerProduct, updateRequest.Id, _logger);

                    switch (account.AccountStatus)
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

    private List<CustomerAddressDto> PopulateCustomerAddresses(UpdateCorporateAccountCommand updateRequest) =>
  new()
    {
            new CustomerAddressDto
            {

            DistrictId = updateRequest.District,
            District = updateRequest.DistrictName,
            CountryId = updateRequest.Country,
            Country = updateRequest.CountryName,
            City = updateRequest.CityName,
            CityId = updateRequest.City,
            Address = updateRequest.Address,
            PostalCode = updateRequest.PostalCode
            }
    };

    private List<CustomerPhoneDto> PopulateCustomerPhones(string code, string number) =>
        new()
        {
            new CustomerPhoneDto
            {
                PhoneCode = code,
                PhoneNumber = number,
                PhoneType = PhoneType.Company,
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

    private List<CustomerEmailDto> PopulateCustomerEmails(string email) =>
    new()
    {
            new CustomerEmailDto
            {
                Email = email,
                EmailType = EmailType.Company,
                Primary = true
            }
    };

    private void UpdateAccountWalletsStatus(EmoneyDbContext dbContext,
            List<Wallet> wallets,
            AccountStatus accountStatus)
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
        }
    }

    private void UpdateAccountUsers(EmoneyDbContext dbContext, List<AccountUser> accountUsers, AccountStatus accountStatus)
    {
        accountUsers.ForEach(accountUser =>
        {
            accountUser.RecordStatus =
                   accountStatus == AccountStatus.Active
                   ? RecordStatus.Active
                   : RecordStatus.Passive;
            accountUser.UpdateDate = DateTime.Now;
            accountUser.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();
        });
    }

    public async Task DeactivateCorporateAccountAsync(DeactivateCorporateAccountCommand request)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var accountId = request.Id;

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var dbCurrentAccount = await dbContext.Account
                .Include(s => s.Wallets)
                .Include(s => s.AccountUsers)
                .Include(s => s.CompanyPool)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == accountId);

            dbCurrentAccount.RecordStatus = RecordStatus.Passive;
            dbCurrentAccount.AccountStatus = AccountStatus.Suspended;
            dbCurrentAccount.ClosingDate = DateTime.Now;

            UpdateAccountWalletsStatus(dbContext,
                dbCurrentAccount.Wallets,
                dbCurrentAccount.AccountStatus
                );


            UpdateAccountUsers(dbContext, dbCurrentAccount.AccountUsers, dbCurrentAccount.AccountStatus);

            dbCurrentAccount.UpdateDate = DateTime.Now;

            dbContext.Update(dbCurrentAccount);

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();

            var companyPool = dbCurrentAccount.CompanyPool;

            var request = new UpdateCorporateAccountCommand
            {
                Email = dbCurrentAccount.Email,
                PhoneCode = dbCurrentAccount.PhoneCode,
                PhoneNumber = dbCurrentAccount.PhoneNumber,
                Address = companyPool.Address,
                City = companyPool.City,
                CityName = companyPool.CityName,
                Country = companyPool.Country,
                CountryName = companyPool.CountryName,
                District = companyPool.District,
                DistrictName = companyPool.DistrictName,
                PostalCode = companyPool.PostalCode
            };

            await UpdateCustomerAsync(dbCurrentAccount, request, true);
        });
    }

    public async Task UpdateCorporateAccountAsync(UpdateCorporateAccountCommand updateRequest)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var dbCurrentAccount = await dbContext.Account
                .Include(s => s.AccountUsers)
                .Include(s => s.CompanyPool)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == updateRequest.Id);

            if (dbCurrentAccount.PhoneCode.ToLowerInvariant() != updateRequest.PhoneCode.ToLowerInvariant())
            {
                dbCurrentAccount.PhoneCode = updateRequest.PhoneCode;
                dbCurrentAccount.CompanyPool.PhoneCode = dbCurrentAccount.PhoneCode;
            }
            if (dbCurrentAccount.PhoneNumber.ToLowerInvariant() != updateRequest.PhoneNumber.ToLowerInvariant())
            {
                dbCurrentAccount.PhoneNumber = updateRequest.PhoneNumber;
                dbCurrentAccount.CompanyPool.PhoneNumber = dbCurrentAccount.PhoneNumber;
            }
            if (dbCurrentAccount.Name != updateRequest.Name)
            {
                dbCurrentAccount.Name = updateRequest.Name;
                dbCurrentAccount.CompanyPool.Title = dbCurrentAccount.Name;
            }
            if (dbCurrentAccount.Email.ToLowerInvariant() != updateRequest.Email.ToLowerInvariant())
            {
                dbCurrentAccount.Email = updateRequest.Email;
                dbCurrentAccount.CompanyPool.Email = dbCurrentAccount.Email;
            }
            if (dbCurrentAccount.CompanyPool.Country != updateRequest.Country)
            {
                dbCurrentAccount.CompanyPool.Country = updateRequest.Country;
            }
            if (dbCurrentAccount.CompanyPool.CountryName != updateRequest.CountryName)
            {
                dbCurrentAccount.CompanyPool.CountryName = updateRequest.CountryName;
            }
            if (dbCurrentAccount.CompanyPool.LandPhone != updateRequest.LandPhone)
            {
                dbCurrentAccount.CompanyPool.LandPhone = updateRequest.LandPhone;
            }
            if (dbCurrentAccount.CompanyPool.WebSiteUrl != updateRequest.WebSiteUrl)
            {
                dbCurrentAccount.CompanyPool.WebSiteUrl = updateRequest.WebSiteUrl;
            }
            if (dbCurrentAccount.CompanyPool.PostalCode != updateRequest.PostalCode)
            {
                dbCurrentAccount.CompanyPool.PostalCode = updateRequest.PostalCode;
            }
            if (dbCurrentAccount.CompanyPool.Address != updateRequest.Address)
            {
                dbCurrentAccount.CompanyPool.Address = updateRequest.Address;
            }
            if (dbCurrentAccount.CompanyPool.City != updateRequest.City)
            {
                dbCurrentAccount.CompanyPool.City = updateRequest.City;
            }
            if (dbCurrentAccount.CompanyPool.CityName != updateRequest.CityName)
            {
                dbCurrentAccount.CompanyPool.CityName = updateRequest.CityName;
            }
            if (dbCurrentAccount.CompanyPool.District != updateRequest.District)
            {
                dbCurrentAccount.CompanyPool.District = updateRequest.District;
            }
            if (dbCurrentAccount.CompanyPool.DistrictName != updateRequest.DistrictName)
            {
                dbCurrentAccount.CompanyPool.DistrictName = updateRequest.DistrictName;
            }

            dbCurrentAccount.UpdateDate = DateTime.Now;

            dbContext.Update(dbCurrentAccount.CompanyPool);
            dbContext.Update(dbCurrentAccount);

            await dbContext.SaveChangesAsync();


            transactionScope.Complete();

            var companyPool = dbCurrentAccount.CompanyPool;

            var request = new UpdateCorporateAccountCommand
            {
                Email = dbCurrentAccount.Email,
                PhoneCode = dbCurrentAccount.PhoneCode,
                PhoneNumber = dbCurrentAccount.PhoneNumber,
                Address = companyPool.Address,
                City = companyPool.City,
                CityName = companyPool.CityName,
                Country = companyPool.Country,
                CountryName = companyPool.CountryName,
                District = companyPool.District,
                DistrictName = companyPool.DistrictName,
                PostalCode = companyPool.PostalCode
            };

            await UpdateCustomerAsync(dbCurrentAccount, request, true);
        });

    }
}
