using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.Accounting.Application.Features.Customers;
using LinkPara.Accounting.Application.Features.Payments;
using LinkPara.Accounting.Application.Features.Payments.Commands.CancelPayment;
using LinkPara.Accounting.Application.Features.Payments.Commands.DeletePayment;
using LinkPara.Accounting.Application.Features.Payments.Commands.PostPayment;
using LinkPara.Accounting.Application.Features.Payments.Queries.GetFilterPayment;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Accounting.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IGenericRepository<Payment> _repository;
    private readonly IMapper _mapper;
    private readonly IAccountingService _accountingService;
    private readonly IContextProvider _contextProvider;
    private readonly ICustomerService _customerService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IGenericRepository<Payment> repository,
        IMapper mapper,
        IAccountingService accountingService,
        IContextProvider contextProvider,
        ICustomerService customerService,
        IAuditLogService auditLogService,
        ILogger<PaymentService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _accountingService = accountingService;
        _contextProvider = contextProvider;
        _customerService = customerService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task CancelPaymentAsync(CancelPaymentCommand request)
    {
        await _accountingService.CancelPaymentAsync(request.ClientReferenceId);
    }

    public async Task<Unit> DeletePaymentAsync(DeletePaymentCommand request)
    {
        var payment = await _repository.GetByIdAsync(request.Id);

        if (payment is null)
        {
            throw new NotFoundException(nameof(payment));
        }
        await _repository.DeleteAsync(payment);

        return Unit.Value;
    }

    public async Task<PaymentDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Payment), id);
        }

        var result = _mapper.Map<PaymentDto>(entity);

        var sourceCustomer = await _customerService.GetCustomerByCodeAsync(result.Source);
        var destinationCustomer = await _customerService.GetCustomerByCodeAsync(result.Destination);

        result.SourceFullName = $"{sourceCustomer.FirstName} {sourceCustomer.LastName}";
        result.DestinationFullName = $"{destinationCustomer.FirstName} {destinationCustomer.LastName}";

        return result;
    }

    public async Task<PaginatedList<PaymentDto>> GetFilterPaymentAsync(GetFilterPaymentQuery request)
    {
        try
        {
            var payments = _repository.GetAll().Where(x => x.RecordStatus == RecordStatus.Active);


            if (!string.IsNullOrEmpty(request.Q))
            {
                payments = payments.Where(b => b.ReferenceId.Contains(request.Q));
            }

            if (request.TransactionDateStart is not null)
            {
                payments = payments.Where(b => b.TransactionDate
                                   >= request.TransactionDateStart);
            }

            if (request.TransactionDateEnd is not null)
            {
                payments = payments.Where(b => b.TransactionDate
                                   <= request.TransactionDateEnd);
            }

            if (request.IsSuccess is not null)
            {
                payments = payments.Where(b => b.IsSuccess == request.IsSuccess);
            }

            if (!string.IsNullOrWhiteSpace(request.Source))
            {
                payments = payments.Where(b => b.Source.Contains(request.Source));
            }

            if (!string.IsNullOrWhiteSpace(request.Destination))
            {
                payments = payments.Where(b => b.Destination.Contains(request.Destination));
            }

            if (request.OperationType is not null)
            {
                payments = payments.Where(b => b.OperationType == request.OperationType);
            }

            var result = await payments
               .PaginatedListWithMappingAsync<Payment, PaymentDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

            var sourceCustomerCodes = result.Items.Select(x => x.Source).ToList();

            var sourceCustomers = await _customerService.GetCustomersByCodesAsync(sourceCustomerCodes);

            var destinationCustomerCodes = result.Items.Select(x => x.Destination).ToList();

            var destinationCustomers = await _customerService.GetCustomersByCodesAsync(destinationCustomerCodes);

            result.Items.ForEach(item =>
            {
                if (!string.IsNullOrWhiteSpace(item.Source)
                && sourceCustomers.TryGetValue(item.Source, out CustomerDto sourceCustomer))
                {

                    item.SourceFullName = $"{sourceCustomer.FirstName} {sourceCustomer.LastName}";
                }

                if (!string.IsNullOrWhiteSpace(item.Destination)
                && destinationCustomers.TryGetValue(item.Destination, out CustomerDto destinationCustomer))
                {

                    item.DestinationFullName = $"{destinationCustomer.FirstName} {destinationCustomer.LastName}";
                }

            });

            return result;
        }
        catch (Exception exception)
        {
            _logger.LogError("Api GetPaymetError Exception : {exception}", exception);
            throw;
        }

    }

    public async Task PostPaymentAsync(PostPaymentCommand request)
    {
        await _accountingService.PostPaymentAsync(new AccountingPayment
        {
            ReferenceId = request.ReferenceId,
            Amount = request.Amount,
            BankCode = request.BankCode,
            BsmvAmount = request.BsmvAmount,
            CommissionAmount = request.CommissionAmount,
            CurrencyCode = request.CurrencyCode,
            Destination = request.Destination,
            HasCommission = request.HasCommission,
            OperationType = request.OperationType,
            Source = request.Source,
            AccountingTransactionType = request.AccountingTransactionType,
            TransactionDate = request.TransactionDate,
            UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
            AccountingCustomerType = request.AccountingCustomerType
        });

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "PostPayment",
            SourceApplication = "Accounting",
            Resource = "AccountingPayment",
            Details = new Dictionary<string, string>
            {
                  { "ReferenceId", request.ReferenceId},
                  { "UserId", Guid.Parse(_contextProvider.CurrentContext.UserId).ToString() },
                  { "BankCode", request.BankCode.ToString() },
            }
        });

    }
}
