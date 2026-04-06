using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Services;

public class AccountingService : IAccountingService
{
    private readonly IBus _bus;
    private readonly ILogger<AccountingService> _logger;
    private readonly IAuditLogService _auditLogService;

    public AccountingService(IBus bus,
                             ILogger<AccountingService> logger,
                             IAuditLogService auditLogService)
    {
        _bus = bus;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    public async Task CreateCustomerAsync(Account account, Wallet wallet, CustomerDto customer)
    {
        try
        {
            if (account == null)
            {
                throw new NotFoundException(nameof(account), wallet.AccountId);
            }

            var nameLastname = NameHelper.ParseName(account.Name);

            var accountingCustomer = new AccountingCustomer
            {
                Code = $"WA-{wallet.WalletNumber}",
                FirstName = nameLastname.FirstName,
                LastName = nameLastname.LastName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                PhoneCode = account.PhoneCode,
                IdentityNumber = account.IdentityNumber,
                CurrencyCode = wallet.CurrencyCode,
                Title = account.Name,
                UserId = Guid.Empty,
                AccountingCustomerType = AccountingCustomerType.Emoney,
                Address = customer.CustomerAddresses?.FirstOrDefault(x => x.Primary)?.Address,
                City = customer.CustomerAddresses?.FirstOrDefault(y => y.Primary)?.City,
                CityCode = customer.CustomerAddresses?.FirstOrDefault(x => x.Primary)?.CityId.ToString(),
                Country = customer.CustomerAddresses?.FirstOrDefault(z => z.Primary)?.Country,
                CountryCode = customer.CustomerAddresses?.FirstOrDefault(x => x.Primary)?.CountryId.ToString(),
                CustomerCode = customer.CustomerNumber.ToString(),
                District = customer.CustomerAddresses?.FirstOrDefault(x => x.Primary)?.District,
                TaxNumber = customer.TaxNumber,
                TaxOffice = customer.TaxAdministration,
                Town = customer.CustomerAddresses?.FirstOrDefault(x => x.Primary)?.District,
            };

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.CreateCustomer"));
            await endpoint.Send(accountingCustomer, tokenSource.Token);

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "CreateCustomer",
                SourceApplication = "Emoney",
                Resource = "AccountingCustomer",
                Details = new Dictionary<string, string>
                {
                       {"Code", wallet.WalletNumber },
                       {"FirstName", nameLastname.FirstName },
                       {"LastName", nameLastname.LastName },
                       {"AccountId", wallet.AccountId.ToString() }
                }
            });

        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnSendMessage detail:\n{exception}");
        }
    }

    public async Task SavePaymentAsync(AccountingPayment payment)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.SavePayment"));
            await endpoint.Send(payment, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnSendMessage detail:\n{exception}");
        }
    }

    public async Task UpdateCustomerAsync(CustomerDto customer)
    {
        try
        {
            var updateAccountingCustomer = new UpdateAccountingCustomer
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                IdentityNumber = customer.IdentityNumber,
                CustomerCode = customer.CustomerNumber.ToString(),
                OldCustomerCode = customer.CustomerNumber.ToString(),
                TaxNumber = customer.TaxNumber,
                PhoneCode = customer.CustomerPhones.FirstOrDefault(cp => cp.Primary)?.PhoneCode,
                PhoneNumber = customer.CustomerPhones.FirstOrDefault(cp => cp.Primary)?.PhoneNumber,
                Title = $"{customer.FirstName} {customer.LastName}",
            };

            if (customer.CustomerAddresses.Any())
            {
                updateAccountingCustomer.Address = customer.CustomerAddresses.FirstOrDefault(ca => ca.Primary).Address;
                updateAccountingCustomer.City = customer.CustomerAddresses.FirstOrDefault(ca => ca.Primary).City;
                updateAccountingCustomer.Country = customer.CustomerAddresses.FirstOrDefault(ca => ca.Primary).Country;
                updateAccountingCustomer.District = customer.CustomerAddresses.FirstOrDefault(ca => ca.Primary).District;
                updateAccountingCustomer.CityCode = customer.CustomerAddresses.FirstOrDefault(ca => ca.Primary).CityId.ToString();
                updateAccountingCustomer.CountryCode = customer.CustomerAddresses.FirstOrDefault(ca => ca.Primary).CountryId.ToString();
            }

            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Accounting.UpdateCustomer"));
            await endpoint.Send(updateAccountingCustomer, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError("UpdateAccountingCustomerError: " + exception);
        }
    }
}
