using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Accounts.Commands.CreateAccount;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Infrastructure.Persistence.Configurations;
using LinkPara.Emoney.Infrastructure.Services;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.SharedModels.BusModels.Commands.Emoney;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Consumers;
public class CreateIndividualKycUserConsumer : IConsumer<CreateIndividualKycUser>
{
    private readonly IUserService _userService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IAccountService _accountService;
    private readonly ICustomerService _customerService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IAccountingService _accountingService;
    private readonly ILogger<AccountingService> _logger;
    public CreateIndividualKycUserConsumer(
        IUserService userService,
        IGenericRepository<AccountUser> accountUserRepository,
        IAccountService accountService,
        ICustomerService customerService,
        IGenericRepository<Account> accountRepository,
        IApplicationUserService applicationUserService,
        IAccountingService accountingService,
        ILogger<AccountingService> logger)
    {
        _userService = userService;
        _accountUserRepository = accountUserRepository;
        _accountService = accountService;
        _customerService = customerService;
        _accountRepository = accountRepository;
        _applicationUserService = applicationUserService;
        _accountingService = accountingService;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<CreateIndividualKycUser> context)
    {
        try
        {
            var user = await _userService.GetAllUsersAsync(new GetUsersRequest
            {
                Email = context.Message.Email,
                PhoneNumber = context.Message.PhoneNumber,
                UserType = UserType.Individual,
                UserStatus = UserStatus.Active
            });

            if (!user.Items.Any())
            {
                var userRequest = new CreateUserRequest
                {
                    Email = context.Message.Email,
                    BirthDate = context.Message.BirthDate,
                    FirstName = context.Message.FirstName,
                    LastName = context.Message.LastName,
                    PhoneCode = context.Message.PhoneCode,
                    PhoneNumber = context.Message.PhoneNumber,
                    UserName = context.Message.UserName,
                    UserType = UserType.Individual,
                    IysPermission = context.Message.IysPermission
                };
                var userResponse = await _userService.CreateUserAsync(userRequest);
                await CreateKycAccount(context.Message, userResponse.UserId);
            }
            else
            {
                var userId = user.Items.FirstOrDefault().Id;
                var accountUser = await GetAccountUser(userId);
                if (IsKycAccount(accountUser.Account.AccountKycLevel)) return;
                await UpdateUser(context.Message, userId);
                await UpdateKycLevelAsync(accountUser, context.Message);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"CreateIndividualKycUserException: {exception.Message}");
        }
    }

    private async Task<AccountUser> GetAccountUser(Guid userId)
    {
        var accountUser = await _accountUserRepository
                .GetAll()
                .Include(s => s.Account)
                .FirstOrDefaultAsync(x => x.UserId == userId &&
                                          x.RecordStatus == RecordStatus.Active);
        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), userId);
        }
        return accountUser;
    }
    private static bool IsKycAccount(AccountKycLevel kycLevel)
    {
        return kycLevel is not AccountKycLevel.NoneKyc and not AccountKycLevel.PreKyc;
    }
    private async Task CreateKycAccount(CreateIndividualKycUser model, Guid userId)
    {
        await _accountService.CreateAccountAsync(new CreateAccountCommand
        {
            AccountKycLevel = AccountKycLevel.Kyc,
            AccountType = AccountType.Individual,
            BirthDate = model.BirthDate,
            Email = model.Email,
            Firstname = model.FirstName,
            IdentityNumber = model.IdentityNumber,
            Lastname = model.LastName,
            PhoneCode = model.PhoneCode,
            PhoneNumber = model.PhoneNumber,
            IdentityUserId = userId
        });
    }
    private async Task UpdateUser(CreateIndividualKycUser model, Guid userId)
    {
        var userPatch = new JsonPatchDocument<PatchUserRequest>();

        userPatch.Replace(x => x.FirstName, model.FirstName);
        userPatch.Replace(x => x.LastName, model.LastName);
        userPatch.Replace(x => x.BirthDate, model.BirthDate);
        userPatch.Replace(x => x.IdentityNumber, model.IdentityNumber);

        await _userService.PatchAsync(userId, userPatch);
    }
    private async Task UpdateKycLevelAsync(AccountUser accountUser, CreateIndividualKycUser model)
    {
        var customer = await _customerService.GetCustomerAsync(accountUser.Account.CustomerId);
        var oldCustomerNumber = customer.CustomerNumber;
        var customerResponse = await UpdateCustomerAsync(customer, model);
        await UpdateAccountAsync(accountUser, model, customerResponse);
        customerResponse.Customer.CustomerNumber = oldCustomerNumber;
        await _accountingService.UpdateCustomerAsync(customerResponse.Customer);
    }
    private async Task UpdateAccountAsync(AccountUser accountUser, CreateIndividualKycUser model, CreateCustomerResponse customerResponse)
    {
        accountUser.Firstname = model.FirstName;
        accountUser.Lastname = model.LastName;
        await _accountUserRepository.UpdateAsync(accountUser);

        var account = accountUser.Account;
        account.IdentityNumber = model.IdentityNumber;
        account.Name = $"{model.FirstName} {model.LastName}";
        account.KycChangeDate = DateTime.Now;
        account.AccountKycLevel = AccountKycLevel.Kyc;

        if (customerResponse.IsChanged)
        {
            account.CustomerId = customerResponse.CustomerId;
        }
        await _accountRepository.UpdateAsync(account);
    }
    private async Task<CreateCustomerResponse> UpdateCustomerAsync(CustomerDto customer, CreateIndividualKycUser model)
    {
        customer.FirstName = model.FirstName;
        customer.LastName = model.LastName;
        customer.BirthDate = model.BirthDate;
        customer.IdentityNumber = model.IdentityNumber;

        var customerRequest = PopulateCustomerRequest(customer, customer.Id);
        return await _customerService.CreateCustomerAsync(customerRequest);
    }
    private CreateCustomerRequest PopulateCustomerRequest(CustomerDto customer, Guid customerId)
    {
        return new CreateCustomerRequest()
        {
            UserId = _applicationUserService.ApplicationUserId,
            CustomerId = customerId,
            CommercialTitle = customer.CommercialTitle,
            TradeRegistrationNumber = customer.TradeRegistrationNumber,
            TaxAdministration = customer.TaxAdministration,
            TaxNumber = customer.TaxNumber,
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
            CreateCustomerProducts = customer.CustomerProducts,
            IdentityNumber = customer.IdentityNumber
        };
    }
}

