using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class CreateAccountCustomerConsumer : IConsumer<CreateAccountCustomer>
{
    private readonly ILogger<CreateAccountCustomerConsumer> _logger;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ICustomerService _customerService;
    private readonly IServiceScopeFactory _scopeFactory;

    public CreateAccountCustomerConsumer(ILogger<CreateAccountCustomerConsumer> logger,
        IApplicationUserService applicationUserService,
        ICustomerService customerService,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _customerService = customerService;
        _scopeFactory = scopeFactory;
    }

    public async Task Consume(ConsumeContext<CreateAccountCustomer> context)
    {
        var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var accountUsers = await dbContext.AccountUser
            .Include(s => s.Account)
            .Where(s =>
                s.Account.CustomerId == Guid.Empty &&
                s.Account.AccountStatus == AccountStatus.Active)
            .ToListAsync();

        foreach (var accountUser in accountUsers)
        {
            if (accountUser.Account.AccountType == AccountType.Individual)
            {
                var customerResponse = await _customerService.CreateCustomerAsync(new CreateCustomerRequest
                {
                    CustomerType = CustomerType.Individual,
                    FirstName = accountUser.Firstname,
                    LastName = accountUser.Lastname,
                    UserId = accountUser.UserId,
                    CreateCustomerProducts = PopulateCustomerProducts(accountUser.AccountId),
                    CreateCustomerEmails = PopulateCustomerEmails(accountUser.Email),
                    CreateCustomerPhones = PopulateCustomerPhones(accountUser.PhoneCode, accountUser.PhoneNumber)
                });

                if (customerResponse.CustomerId != Guid.Empty)
                {
                    accountUser.Account.UpdateDate = DateTime.Now;
                    accountUser.Account.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                    accountUser.Account.CustomerId = customerResponse.CustomerId;

                    dbContext.Update(accountUser.Account);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    _logger.LogError($"Customer Could Not Be Created - AccountId : {accountUser.AccountId}");
                }
            }
        }
    }

    private List<CustomerProductDto> PopulateCustomerProducts(Guid accountId) =>
        new()
        {
            new CustomerProductDto
            {
                AccountId = accountId,
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

}

