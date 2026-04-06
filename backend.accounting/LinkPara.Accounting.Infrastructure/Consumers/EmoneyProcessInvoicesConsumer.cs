using LinkPara.Accounting.Application.Commons.Interfaces;
using LinkPara.Accounting.Application.Commons.Models;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.Accounting.Domain.Enums;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkPara.Accounting.Infrastructure.Consumers;

public class EmoneyProcessInvoicesConsumer : IConsumer<ProcessCustomerInvoices>
{
    private readonly IAccountingService _accountingService;
    private readonly IConfiguration _configuration;
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly ILogger<EmoneyProcessInvoicesConsumer> _logger;

    public EmoneyProcessInvoicesConsumer(IAccountingService accountingService,
        IConfiguration configuration,
        IGenericRepository<Payment> paymentRepository,
        ILogger<EmoneyProcessInvoicesConsumer> logger)
    {
        _accountingService = accountingService;
        _configuration = configuration;
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessCustomerInvoices> context)
    {
        try
        {
            var registerInvoiceDayCount = _configuration.GetValue<string>("RegisterInvoiceDayCount");

            if (!int.TryParse(registerInvoiceDayCount, out var dayCount) || dayCount <= 0)
            {
                dayCount = 7;
            }


            List<Payment> payments = await GetPaymentsAsync(dayCount);

            var senderCommissionPayments = PrepareSenderCommissionPayments(payments);
            var receiverCommissionPayments = PrepareReceiverCommissionPayments(payments);

            foreach (var item in senderCommissionPayments)
            {
                try
                {
                    var registerInvoiceId = await _accountingService.ProcessInvoiceAsync(new ProcessInvoiceRequest
                    {
                        CommissionType = CommissionType.Sender,
                        CustomerCode = item.Value.FirstOrDefault().Source,
                        Payments = item.Value,
                        TransactionType = AccountingTransactionType.Emoney
                    });
                    item.Value.ForEach(p =>
                    {
                        p.SenderInvoiceId = registerInvoiceId;
                        p.SenderPaymentInvoiceStatus = PaymentInvoiceStatus.Processed;
                    });
                    await _paymentRepository.UpdateRangeAsync(item.Value);
                }
                catch (Exception exception)
                {
                    _logger.LogError("Error On ProcessInvoice exception : {Exception}", exception);
                }
            }

            foreach (var item in receiverCommissionPayments)
            {
                try
                {
                    var registerInvoiceId = await _accountingService.ProcessInvoiceAsync(new ProcessInvoiceRequest
                    {
                        CommissionType = CommissionType.Receiver,
                        CustomerCode = item.Value.FirstOrDefault().Destination,
                        Payments = item.Value,
                        TransactionType = AccountingTransactionType.Emoney
                    });

                    item.Value.ForEach(p =>
                    {
                        p.ReceiverInvoiceId = registerInvoiceId;
                        p.ReceiverPaymentInvoiceStatus = PaymentInvoiceStatus.Processed;
                    });

                    await _paymentRepository.UpdateRangeAsync(item.Value);
                }
                catch (Exception exception)
                {
                    _logger.LogError("Error On ProcessInvoice exception : {Exception}", exception);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("Error On EmoneyProcessInvoicesConsumer Exception : {exception}", exception);
        }

    }

    private async Task<List<Payment>> GetPaymentsAsync(int dayCount)
    {
        var transactionDate = DateTime.Now.AddDays(-dayCount).Date;
        var payments = await _paymentRepository.GetAll()
            .Where(x => x.TransactionDate > transactionDate
                     && x.AccountingTransactionType == AccountingTransactionType.Emoney
                     && x.IsSuccess
                     && x.RecordStatus == RecordStatus.Active
                     && (x.CommissionAmount > 0 || x.ReceiverCommissionAmount > 0)
                     && (x.SenderPaymentInvoiceStatus != PaymentInvoiceStatus.Processed || x.ReceiverPaymentInvoiceStatus != PaymentInvoiceStatus.Processed))
            .ToListAsync();
        return payments;
    }


    private static Dictionary<string, List<Payment>> PrepareSenderCommissionPayments(List<Payment> payments)
    {
        var walletPayments = new Dictionary<string, List<Payment>>();
        var senderCommissionpayments = payments.Where(x => x.CommissionAmount > 0
                                                        && x.SenderPaymentInvoiceStatus != PaymentInvoiceStatus.Processed).ToList();

        var senderGroupByWalletNumber = senderCommissionpayments.GroupBy(x => x.Source);

        foreach (var item in senderGroupByWalletNumber)
        {
            if (!walletPayments.Any(x => x.Key == item.Key))
            {
                walletPayments.Add(item.Key, item.ToList());
            }
            else
            {
                walletPayments[item.Key].AddRange(item.ToList());
            }
        }
        return walletPayments;
    }

    private static Dictionary<string, List<Payment>> PrepareReceiverCommissionPayments(List<Payment> payments)
    {
        var walletPayments = new Dictionary<string, List<Payment>>();
        var receviverCommissionpayments = payments.Where(x => x.ReceiverCommissionAmount > 0
                                                           && x.ReceiverPaymentInvoiceStatus != PaymentInvoiceStatus.Processed).ToList();

        var receiverGroupByWalletNumber = receviverCommissionpayments.GroupBy(x => x.Destination);

        foreach (var item in receiverGroupByWalletNumber)
        {
            if (!walletPayments.Any(x => x.Key == item.Key))
            {
                walletPayments.Add(item.Key, item.ToList());
            }
            else
            {
                walletPayments[item.Key].AddRange(item.ToList());
            }
        }
        return walletPayments;
    }

}
