using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services
{
    public class AccountingService : IAccountingService
    {
        private readonly IBus _bus;
        private readonly ILogger<AccountingService> _logger;
        private readonly IParameterService _parameterService;
        
        public const string DefaultCustomerCodeInitial = "ME-";

        public AccountingService(IBus bus,
                                 ILogger<AccountingService> logger, IParameterService parameterService)
        {
            _bus = bus;
            _logger = logger;
            _parameterService = parameterService;
        }
        public async Task CreateCustomerAsync(Merchant merchant,Customer customer, ContactPerson contactPerson)
        {
            try
            {
                var customerCodeInitial = await GetCustomerCodeInitialAsync();
                customer.AuthorizedPerson = contactPerson;
                merchant.Customer = customer;
                var companyType = customer.CompanyType;

                var accountingCustomer = new AccountingCustomer
                {
                    Code = $"{customerCodeInitial}{merchant.Number}",
                    FirstName = companyType == CompanyType.Individual ?
                                    merchant.Customer?.AuthorizedPerson.Name :
                                    string.Empty,
                    LastName = companyType == CompanyType.Individual ?
                                    merchant.Customer?.AuthorizedPerson.Surname :
                                    string.Empty,
                    Email = merchant.Customer?.AuthorizedPerson.Email,
                    PhoneNumber = merchant.Customer?.AuthorizedPerson.MobilePhoneNumber,
                    PhoneCode = merchant.PhoneCode,
                    IdentityNumber = companyType == CompanyType.Individual ?
                                        merchant.Customer?.AuthorizedPerson.IdentityNumber :
                                        string.Empty,
                    CurrencyCode = "TRY",
                    Title = merchant.Customer?.CommercialTitle,
                    UserId = merchant.Id,
                    AccountingCustomerType = AccountingCustomerType.PF,
                    City = merchant.Customer?.CityName,
                    CityCode = merchant.Customer?.City.ToString(),
                    Country = merchant.Customer?.CountryName,
                    CountryCode = merchant.Customer?.Country.ToString(),
                    Address = merchant.Customer?.Address,
                    TaxNumber = companyType != CompanyType.Individual ?
                                    merchant.Customer?.TaxNumber :
                                    string.Empty,
                    TaxOffice = merchant.Customer?.TaxAdministration,
                    District = merchant.Customer?.DistrictName,
                    CustomerCode = merchant.Customer?.CustomerNumber.ToString(),

                };

                using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.CreateCustomer"));
                await endpoint.Send(accountingCustomer, tokenSource.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError($"ExceptionOnSendMessage detail:\n{exception}");
            }
        }

        public async Task<string> GetCustomerCodeInitialAsync()
        {
            try
            {
                var accountingCustomerInitial = await _parameterService.GetParameterAsync("PFParameters", "AccountingCustomerCodeInitial");
                return accountingCustomerInitial is not null ? accountingCustomerInitial.ParameterValue : DefaultCustomerCodeInitial;
            }
            catch (Exception exception)
            {
                _logger.LogError($"AccountingCustomerCodeInitial not found. ContinueWithDefaultValues: {exception}");
                return DefaultCustomerCodeInitial;
            }
        }
    }
}