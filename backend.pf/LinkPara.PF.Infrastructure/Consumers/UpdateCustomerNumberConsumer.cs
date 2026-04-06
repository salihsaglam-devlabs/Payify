using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.PF.Application.Commons.Models.Customers;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class UpdateCustomerNumberConsumer : IConsumer<UpdateCustomerNumber>
{
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly ILogger<UpdateCustomerNumberConsumer> _logger;
    private readonly IAuditLogService _auditLogService;
    
    public UpdateCustomerNumberConsumer(
        IGenericRepository<Customer> customerRepository,
        ILogger<UpdateCustomerNumberConsumer> logger,
        IAuditLogService auditLogService)
    {
        _customerRepository = customerRepository;
        _logger = logger;
        _auditLogService = auditLogService;
    }
    
    public async Task Consume(ConsumeContext<UpdateCustomerNumber> context)
    {
        try
        {
            var customer = await _customerRepository.GetAll()
                .FirstOrDefaultAsync(s => s.Id == context.Message.CustomerId);

            if (customer is null)
            {
                throw new NotFoundException(nameof(Customer), context.Message.CustomerId);
            }

            customer.CustomerId = context.Message.CustomerManagementId;
            customer.CustomerNumber = context.Message.CustomerNumber;
            await _customerRepository.UpdateAsync(customer);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateCustomerNumberConsumer",
                    SourceApplication = "PF",
                    Resource = "Customer",
                    UserId = Guid.Empty,
                    Details = new Dictionary<string, string>
                    {
                        { "CustomerId", context.Message.CustomerId.ToString() },
                        { "CustomerManagementId", context.Message.CustomerManagementId.ToString() },
                        { "CustomerNumber", context.Message.CustomerNumber.ToString() },
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateCustomerNumberConsumer " +
                             $"CustomerId: {context.Message.CustomerId}, " +
                             $"CustomerManagementId: {context.Message.CustomerManagementId}, " +
                             $"CustomerNumber: {context.Message.CustomerNumber}, Error : {exception}");
            throw;
        }
    }
}