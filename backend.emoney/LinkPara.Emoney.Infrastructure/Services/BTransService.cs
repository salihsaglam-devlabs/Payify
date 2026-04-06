using System.Globalization;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.BTransModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Services;

public class BTransService : IBTransService
{
    private readonly IBus _bus;
    private readonly ILogger<BTransService> _logger;
    private readonly ICustomerService _customerService;

    public BTransService(IBus bus,
        ILogger<BTransService> logger,
        ICustomerService customerService)
    {
        _bus = bus;
        _logger = logger;
        _customerService = customerService;
    }
    
    public async Task SaveMoneyTransferAsync(MoneyTransferReport moneyTransfer)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:BTrans.SaveMoneyTransfer"));
            await endpoint.Send(moneyTransfer, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnSendMessage detail:\n{exception}");
        }
    }
    
    public async Task SaveBalanceInformationAsync(BalanceInformationReport balanceInformation)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:BTrans.SaveBalanceInformation"));
            await endpoint.Send(balanceInformation, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnSendMessage detail:\n{exception}");
        }
    }

    public async Task<BTransIdentity> GetCustomerInformationAsync(Guid customerId)
    {
        try
        {
            var customer = await _customerService.GetCustomerAsync(customerId);
            var address = customer.CustomerAddresses.FirstOrDefault();
            var phone = customer.CustomerPhones.FirstOrDefault();
            var email = customer.CustomerEmails.FirstOrDefault();
            var bTransIdentity = new BTransIdentity
            {
                IsSucceed = true,
                IsCorporate = customer.CustomerType != CustomerType.Individual,
                PhoneNumber = string.Concat(phone.PhoneCode, phone.PhoneNumber),
                Email = email.Email,
                NationCountryId = address?.CountryIso2,
                FullAddress = address?.Address,
                District = address?.District,
                PostalCode = address?.PostalCode,
                CityId = !string.IsNullOrEmpty(address?.CityIso2) ? Convert.ToInt32(address.CityIso2) : 0,
                City = address?.City
            };
            if (bTransIdentity.IsCorporate)
            {
                bTransIdentity.TaxNumber = customer.TaxNumber;
                bTransIdentity.CommercialTitle = customer.CommercialTitle;
            }
            else
            {
                bTransIdentity.FirstName = customer.FirstName.ToUpper(new CultureInfo("tr-TR"));
                bTransIdentity.LastName = customer.LastName.ToUpper(new CultureInfo("tr-TR"));
                bTransIdentity.DocumentType = (int?)customer.DocumentType;
                bTransIdentity.IdentityNumber = customer.IdentityNumber;
            }
            return bTransIdentity;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Cannot fetch customer[{customerId}] for BTrans reporting tool  Error : {exception}");
            return new BTransIdentity
            {
                IsSucceed = false
            };
        }
    }

    public BTransIdentity GetAccountInformation(Account account)
    {
        var bTransIdentity = new BTransIdentity
        {
            IsSucceed = true,
            IsCorporate = account.AccountType != AccountType.Individual,
            PhoneNumber = string.Concat(account.PhoneCode, account.PhoneNumber),
            Email = account.Email,
        };
        if (bTransIdentity.IsCorporate)
        {
            bTransIdentity.TaxNumber = account.IdentityNumber;
            bTransIdentity.CommercialTitle = account.Name;
        }
        else
        {
            var accountNames = NameHelper.ParseName(account.Name);
            bTransIdentity.FirstName = accountNames.FirstName.ToUpper(new CultureInfo("tr-TR"));
            bTransIdentity.LastName = accountNames.LastName.ToUpper(new CultureInfo("tr-TR"));
            bTransIdentity.IdentityNumber = account.IdentityNumber;
        }
        return bTransIdentity;
    }
}