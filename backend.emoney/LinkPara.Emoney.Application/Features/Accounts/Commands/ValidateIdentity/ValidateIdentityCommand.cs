using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.HttpProviders.KPS;
using LinkPara.HttpProviders.KPS.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LinkPara.Emoney.Application.Features.Accounts.Commands.ValidateIdentity;

public class ValidateIdentityCommand : IRequest
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public long IdentityNo { get; set; }
    public string Profession { get; set; }
}

public class ValidateIdentityCommandHandler : IRequestHandler<ValidateIdentityCommand>
{
    private readonly IKpsService _kpsService;
    private readonly ILogger<ValidateIdentityCommandHandler> _logger;
    private readonly ITierLevelService _tierLevelService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly ICustomerService _customerService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IUserService _userService;
    private readonly IParameterService _parameterService;
    private readonly IStringLocalizer _localizer;
    private readonly ISearchService _searchService;
    private readonly IBus _bus;
    private const int MatchRate = 90;

    public ValidateIdentityCommandHandler(
        IKpsService kpsService,
        ILogger<ValidateIdentityCommandHandler> logger,
        ITierLevelService tierLevelService,
        IGenericRepository<AccountUser> accountUserRepository,
        IGenericRepository<Account> accountRepository,
        ICustomerService customerService,
        IApplicationUserService applicationUserService,
        IUserService userService,
        IParameterService parameterService,
        IStringLocalizerFactory factory,
        ISearchService searchService,
        IBus bus)
    {
        _kpsService = kpsService;
        _logger = logger;
        _tierLevelService = tierLevelService;
        _accountUserRepository = accountUserRepository;
        _accountRepository = accountRepository;
        _customerService = customerService;
        _applicationUserService = applicationUserService;
        _userService = userService;
        _parameterService = parameterService;
        _searchService = searchService;
        _localizer = factory.Create("Exceptions", "LinkPara.Emoney.API");
        _bus = bus;
    }

    public async Task<Unit> Handle(ValidateIdentityCommand command, CancellationToken cancellationToken)
    {
        var accountUser = await _accountUserRepository.GetAll()
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s =>
                s.UserId == command.UserId &&
                s.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

        if (accountUser is null)
        {
            throw new NotFoundException(command.UserId.ToString());
        }

        var checkIdentityExists = await _accountRepository
            .GetAll()
            .Where
            (
                a =>
                    a.Id != accountUser.AccountId &&
                    a.IdentityNumber == command.IdentityNo.ToString() &&
                    a.AccountKycLevel != AccountKycLevel.NoneKyc &&
                    a.RecordStatus == RecordStatus.Active
            )
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (checkIdentityExists is not null)
        {
            throw new DuplicateRecordException();
        }

        var kpsResponse = await GetKpsInformationAsync(command);

        await IsBirthdateBetweenAllowedRangeAsync(Convert.ToDateTime(kpsResponse.IDRegistration.BirthDate));

        var isBlacklistCheckEnabled =
            (await _parameterService.GetParameterAsync("BlackListParameters", "CheckAtKps")).ParameterValue == "1";


        if (isBlacklistCheckEnabled)
        {
            await CheckBlackListAsync(kpsResponse);
        }
        
        var customer = await _customerService.GetCustomerAsync(accountUser.Account.CustomerId);
        
        var oldCustomerNumber = customer.CustomerNumber;

        await UpdateKycLevelAsync(accountUser, kpsResponse);

        var customerResponse = await UpdateCustomerAsync(accountUser, kpsResponse, command.Profession);

        await UpdateIdentityAsync(accountUser, kpsResponse);

        await UpdateAccountingCustomerAsync(customerResponse, oldCustomerNumber);

        return Unit.Value;
    }

    private async Task UpdateAccountingCustomerAsync(CreateCustomerResponse createCustomerResponse, int oldCustomerNumber)
    {
        try
        {
            var updateAccountingCustomer = new UpdateAccountingCustomer
            {
                FirstName = createCustomerResponse.Customer.FirstName,
                LastName = createCustomerResponse.Customer.LastName,
                IdentityNumber = createCustomerResponse.Customer.IdentityNumber,
                OldCustomerCode = oldCustomerNumber.ToString(),
                CustomerCode = createCustomerResponse.CustomerNumber.ToString(),
                TaxNumber = createCustomerResponse.Customer.TaxNumber,
                PhoneCode = createCustomerResponse.Customer.CustomerPhones.FirstOrDefault(cp => cp.Primary)?.PhoneCode,
                PhoneNumber = createCustomerResponse.Customer.CustomerPhones.FirstOrDefault(cp => cp.Primary)?.PhoneNumber,
                Title = $"{createCustomerResponse.Customer.FirstName} {createCustomerResponse.Customer.LastName}",
            };

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.UpdateCustomer"));
            await endpoint.Send(updateAccountingCustomer, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError("UpdateAccountingCustomerError: " + exception);
        }
    }

    private async Task<KpsResponse> GetKpsInformationAsync(ValidateIdentityCommand command)
    {
        var identity = await _kpsService.GetPersonalInformation(
            new KpsServiceRequest
            {
                Birthday = command.DateOfBirth.Day,
                BirthMonth = command.DateOfBirth.Month,
                YearOfBirth = command.DateOfBirth.Year,
                TcNo = command.IdentityNo
            }
        );

        if (identity?.IDRegistration is null)
        {
            _logger.LogError(
                $"KPS validation failed exception: Message : KPS validation failed");

            throw new KpsValidationFailedException();
        }

        if (!CheckPersonalInformation(command.FirstName, identity.IDRegistration.Name) ||
            !CheckPersonalInformation(command.LastName, identity.IDRegistration.Surname))
        {
            _logger.LogError($"Personal information do not match Error: KpsFirstName : {identity.IDRegistration.Name} - " +
                             $"KpsLastName : {identity.IDRegistration.Surname}");

            throw new KPSInformationsNotMatchException();
        }

        return identity;
    }

    private static bool CheckPersonalInformation(string value, string kpsValue)
    {
        Regex whiteSpace = new Regex(@"\s+");

        var trimmedValue = whiteSpace.Replace(value, "").ToLower(CultureInfo.CurrentCulture);
        var trimmedKpsValue = whiteSpace.Replace(kpsValue, "").ToLower(CultureInfo.CurrentCulture);

        return trimmedKpsValue.Contains(trimmedValue);
    }

    private async Task UpdateKycLevelAsync(AccountUser accountUser, KpsResponse kpsResponse)
    {
        await _tierLevelService.CheckOrUpgradeAccountTierAsync(accountUser.Account, AccountTierValidation.Identity);

        accountUser.Account.IdentityNumber = kpsResponse.IDRegistration.IdentityNo;
        accountUser.Account.Name = $"{kpsResponse.IDRegistration.Name} {kpsResponse.IDRegistration.Surname}";
        accountUser.Firstname = kpsResponse.IDRegistration.Name;
        accountUser.Lastname = kpsResponse.IDRegistration.Surname;

        await _accountUserRepository.UpdateAsync(accountUser);
        await _accountRepository.UpdateAsync(accountUser.Account);
    }

    private async Task<CreateCustomerResponse> UpdateCustomerAsync(AccountUser accountUser, KpsResponse kpsResponse, string profession)
    {
        var customer = await _customerService.GetCustomerAsync(accountUser.Account.CustomerId);

        customer.FirstName = kpsResponse.IDRegistration.Name;
        customer.LastName = kpsResponse.IDRegistration.Surname;
        customer.BirthDate = Convert.ToDateTime(kpsResponse.IDRegistration.BirthDate);
        customer.IdentityNumber = kpsResponse.IDRegistration.IdentityNo;
        customer.Profession = profession;

        var customerRequest = PopulateCustomerRequest(customer, accountUser.Account.CustomerId);
        var customerUpdate = await _customerService.CreateCustomerAsync(customerRequest);
        if (customerUpdate.IsChanged)
        {
            accountUser.Account.CustomerId = customerUpdate.CustomerId;
            await _accountRepository.UpdateAsync(accountUser.Account);
        }
        return customerUpdate;
    }

    private async Task UpdateIdentityAsync(AccountUser accountUser, KpsResponse kpsResponse)
    {
        var userPatch = new JsonPatchDocument<PatchUserRequest>();

        userPatch.Replace(x => x.FirstName, kpsResponse.IDRegistration.Name);
        userPatch.Replace(x => x.LastName, kpsResponse.IDRegistration.Surname);
        userPatch.Replace(x => x.BirthDate, Convert.ToDateTime(kpsResponse.IDRegistration.BirthDate));
        userPatch.Replace(x => x.IdentityNumber, kpsResponse.IDRegistration.IdentityNo);

        await _userService.PatchAsync(accountUser.UserId, userPatch);
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
            DocumentType = (DocumentType)customer.DocumentType,
            SerialNumber = customer.SerialNumber,
            BirthDate = customer.BirthDate,
            Profession = customer.Profession,
            NationCountryId = customer.NationCountryId,
            NationCountry = customer.NationCountry,
            CustomerType = customer.CustomerType,
            CreateCustomerProducts = customer.CustomerProducts,
            IdentityNumber = customer.IdentityNumber
        };
    }

    private async Task IsBirthdateBetweenAllowedRangeAsync(DateTime dt)
    {
        var customerAgeRequirements = await _parameterService.GetParametersAsync("CustomerAgeRequirements");

        _ = int.TryParse(
            customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinAge")?.ParameterValue, out var minAge);
        var rangeStart = DateTime.Now.AddYears(-1 * minAge);

        _ = int.TryParse(
            customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MaxAge")?.ParameterValue, out var maxAge);
        var rangeEnd = DateTime.Now.AddYears(-1 * maxAge);

        if (!(dt <= rangeStart && dt >= rangeEnd))
        {
            var exceptionMessage = _localizer.GetString("BirthdateOutOfRange")
                .Value.Replace("@@minAge",
                    customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinAge")?.ParameterValue);

            throw new BirthdateOutOfRangeException(exceptionMessage);
        }
    }

    private async Task CheckBlackListAsync(KpsResponse kpsResponse)
    {
        SearchByNameRequest searchRequest = new()
        {
            Name = $"{kpsResponse.IDRegistration.Name} {kpsResponse.IDRegistration.Surname}",
            BirthYear = Convert.ToDateTime(kpsResponse.IDRegistration.BirthDate).Year.ToString(),
            SearchType = SearchType.Any,
            FraudChannelType = FraudChannelType.Web
        };
        var blackListControl = await _searchService.GetSearchByName(searchRequest);
        if ((blackListControl.MatchStatus == MatchStatus.PotentialMatch || blackListControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListControl.MatchRate >= MatchRate)
        {
            var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

            var exceptionMessage = _localizer.GetString("UserInBlacklistException");

            throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
        }
    }
}