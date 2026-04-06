using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Application.Features.Payments.Commands.Return;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Services.VposServices;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Consumers;

public class ProcessTimeoutItemsConsumer : IConsumer<TimeoutTransaction>
{
    private readonly ILogger<ProcessTimeoutItemsConsumer> _logger;
    private readonly IGenericRepository<TimeoutTransaction> _timeoutTransactionRepository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
    private readonly IOrderNumberGeneratorService _orderNumberGeneratorService;
    private readonly ICurrencyService _currencyService;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly VposServiceFactory _vposServiceFactory;
    private readonly IOnUsPaymentService _onUsPaymentService;

    public ProcessTimeoutItemsConsumer(ILogger<ProcessTimeoutItemsConsumer> logger,
        IGenericRepository<TimeoutTransaction> timeoutTransactionRepository,
        IGenericRepository<Vpos> vposRepository,
        IOrderNumberGeneratorService orderNumberGeneratorService,
        VposServiceFactory vposServiceFactory,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<BankTransaction> bankTransactionRepository,
        ICurrencyService currencyService,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IOnUsPaymentService onUsPaymentService, IGenericRepository<MerchantVpos> merchantVposRepository)
    {
        _logger = logger;
        _timeoutTransactionRepository = timeoutTransactionRepository;
        _vposRepository = vposRepository;
        _orderNumberGeneratorService = orderNumberGeneratorService;
        _vposServiceFactory = vposServiceFactory;
        _merchantRepository = merchantRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _currencyService = currencyService;
        _merchantTransactionRepository = merchantTransactionRepository;
        _onUsPaymentService = onUsPaymentService;
        _merchantVposRepository = merchantVposRepository;
    }
    public async Task Consume(ConsumeContext<TimeoutTransaction> timeoutTransactionContext) => await ProcessTimeoutTransactionAsync(timeoutTransactionContext);

    private async Task ProcessTimeoutTransactionAsync(ConsumeContext<TimeoutTransaction> timeoutTransactionContext, int tryCount = 1)
    {
        var message = timeoutTransactionContext.Message;

        var timeoutTransaction = await _timeoutTransactionRepository.GetAll()
            .SingleOrDefaultAsync(w => w.Id == message.Id);

        if (timeoutTransaction is null)
            return;

        var vpos = await _vposRepository.GetAll().Include(b => b.AcquireBank)
            .Include(b => b.MerchantVposList.Where(b => b.RecordStatus == RecordStatus.Active &&
                     b.MerchantId == timeoutTransaction.MerchantId))
            .Include(b => b.VposBankApiInfos).ThenInclude(s => s.Key)
            .FirstOrDefaultAsync(b => b.Id == timeoutTransaction.VposId);

        if (vpos is null)
            return;

        var merchantTransaction = await _merchantTransactionRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == timeoutTransaction.MerchantTransactionId);
        
        var referenceBankTransaction = await _bankTransactionRepository.GetAll()
            .FirstOrDefaultAsync(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.OrderId == timeoutTransaction.OrderId);
        
        var merchantVpos = await _merchantVposRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.RecordStatus == RecordStatus.Active &&
                                      s.TerminalStatus == TerminalStatus.Active &&
                                      s.VposId == vpos.Id &&
                                      s.MerchantId == timeoutTransaction.MerchantId);

        try
        {
            var posResponse = new PosPaymentDetailResponse();
            var subMerchantInfo = await GetSubMerchant(timeoutTransaction.MerchantId);
            var bankService = _vposServiceFactory.GetVposServices(vpos, timeoutTransaction.MerchantId, merchantTransaction.IsInsurancePayment);

            if (merchantTransaction.IsOnUsPayment)
            {
                posResponse = await _onUsPaymentService.GetPaymentDetailAsync(timeoutTransaction.OriginalOrderId);
            }
            else
            {
                posResponse = await bankService.PaymentDetail(new PosPaymentDetailRequest
                {
                    OrderNumber = timeoutTransaction.OriginalOrderId,
                    LanguageCode = timeoutTransaction.LanguageCode,
                    Currency = timeoutTransaction.Currency,
                    StartDate = DateTime.Now.AddMonths(-3),
                    EndDate = DateTime.Now,
                    SubMerchantCode = vpos.MerchantVposList?.FirstOrDefault()?.SubMerchantCode,
                    Password = vpos.MerchantVposList?.FirstOrDefault()?.Password,
                    OrderDate = timeoutTransaction.TransactionDate,
                    RRN = referenceBankTransaction?.RrnNumber,
                    ServiceProviderPspMerchantId = merchantVpos.ServiceProviderPspMerchantId
                });
            }

            if (posResponse.IsSuccess)
            {
                var oldTimeOutStatus = timeoutTransaction.TimeoutTransactionStatus;
                timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.NoActionNeeded;

                if (posResponse.OrderStatus is OrderStatus.Rejected or OrderStatus.OrderNotFound)
                {
                    timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.NoActionNeeded;
                }
                else if (posResponse.OrderStatus == OrderStatus.Cancelled)
                {
                    timeoutTransaction.TimeoutTransactionStatus =
                        timeoutTransaction.TransactionType == TransactionType.Reverse
                            ? TimeoutTransactionStatus.CancelFail
                            : TimeoutTransactionStatus.NoActionNeeded;
                }
                else if (posResponse.OrderStatus == OrderStatus.PreAuth)
                {
                    if (timeoutTransaction.TransactionType == TransactionType.PreAuth)
                    {
                        var cancelResponse = await PosVoid(bankService, timeoutTransaction, subMerchantInfo, referenceBankTransaction, merchantVpos);

                        if (cancelResponse.IsSuccess)
                        {
                            timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Canceled;
                            timeoutTransaction.ResponseCode = cancelResponse.ResponseCode;
                            timeoutTransaction.ResponseMessage = cancelResponse.ResponseMessage;
                            timeoutTransaction.OrderId = cancelResponse.OrderNumber;
                        }
                        else
                        {
                            timeoutTransaction.ErrorCode = cancelResponse.ResponseCode;
                            timeoutTransaction.ErrorMessage = cancelResponse.ResponseMessage;
                            await Retry(timeoutTransaction);
                        }
                    }
                }
                else if (posResponse.OrderStatus == OrderStatus.PostAuth)
                {
                    if (timeoutTransaction.TransactionType is TransactionType.PostAuth or TransactionType.Auth)
                    {
                        var cancelResponse = await PosVoid(bankService, timeoutTransaction, subMerchantInfo, referenceBankTransaction, merchantVpos);

                        if (cancelResponse.IsSuccess)
                        {
                            timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Canceled;
                            timeoutTransaction.ResponseCode = cancelResponse.ResponseCode;
                            timeoutTransaction.ResponseMessage = cancelResponse.ResponseMessage;
                            timeoutTransaction.OrderId = cancelResponse.OrderNumber;
                        }
                        else
                        {
                            var refundResponse = await PosRefund(bankService, timeoutTransaction, subMerchantInfo, merchantTransaction, referenceBankTransaction, merchantVpos);

                            if (refundResponse.IsSuccess)
                            {
                                timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Refunded;
                                timeoutTransaction.ResponseCode = refundResponse.ResponseCode;
                                timeoutTransaction.ResponseMessage = refundResponse.ResponseMessage;
                                timeoutTransaction.OrderId = refundResponse.OrderNumber;
                            }
                            else
                            {
                                timeoutTransaction.ErrorCode = refundResponse.ResponseCode;
                                timeoutTransaction.ErrorMessage = refundResponse.ResponseMessage;
                                await Retry(timeoutTransaction);
                            }
                        }
                    }
                }
                else if (posResponse.OrderStatus == OrderStatus.EndOfDayCompleted)
                {
                    if (timeoutTransaction.TransactionType is TransactionType.Auth or TransactionType.PostAuth)
                    {
                        var refundResponse = await PosRefund(bankService, timeoutTransaction, subMerchantInfo, merchantTransaction, referenceBankTransaction, merchantVpos);

                        if (refundResponse.IsSuccess)
                        {
                            timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Refunded;
                            timeoutTransaction.ResponseCode = refundResponse.ResponseCode;
                            timeoutTransaction.ResponseMessage = refundResponse.ResponseMessage;
                            timeoutTransaction.OrderId = refundResponse.OrderNumber;
                        }
                        else
                        {
                            timeoutTransaction.ErrorCode = refundResponse.ResponseCode;
                            timeoutTransaction.ErrorMessage = refundResponse.ResponseMessage;
                            await Retry(timeoutTransaction);
                        }
                    }
                }
                else if (posResponse.OrderStatus == OrderStatus.WaitingEndOfDay)
                {
                    if (timeoutTransaction.TransactionType is TransactionType.Auth or TransactionType.PostAuth)
                    {
                        var cancelResponse = await PosVoid(bankService, timeoutTransaction, subMerchantInfo, referenceBankTransaction, merchantVpos);

                        if (cancelResponse.IsSuccess)
                        {
                            timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Canceled;
                            timeoutTransaction.ResponseCode = cancelResponse.ResponseCode;
                            timeoutTransaction.ResponseMessage = cancelResponse.ResponseMessage;
                            timeoutTransaction.OrderId = cancelResponse.OrderNumber;
                        }
                        else
                        {
                            var refundResponse = await PosRefund(bankService, timeoutTransaction, subMerchantInfo, merchantTransaction, referenceBankTransaction, merchantVpos);

                            if (refundResponse.IsSuccess)
                            {
                                timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Refunded;
                                timeoutTransaction.ResponseCode = refundResponse.ResponseCode;
                                timeoutTransaction.ResponseMessage = refundResponse.ResponseMessage;
                                timeoutTransaction.OrderId = refundResponse.OrderNumber;
                            }
                            else
                            {
                                timeoutTransaction.ErrorCode = refundResponse.ResponseCode;
                                timeoutTransaction.ErrorMessage = refundResponse.ResponseMessage;
                                await Retry(timeoutTransaction);
                            }
                        }
                    }
                }
                else if (posResponse.OrderStatus == OrderStatus.Refunded)
                {
                    if (timeoutTransaction.TransactionType == TransactionType.Return)
                    {
                        if (vpos.IsOnUsVpos)
                        {
                            timeoutTransaction.RetryCount = 6;
                            await Retry(timeoutTransaction);
                        }
                        else
                        {
                            var cancelResponse = await PosVoid(bankService, timeoutTransaction, subMerchantInfo, referenceBankTransaction, merchantVpos);

                            if (cancelResponse.IsSuccess)
                            {
                                timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Canceled;
                                timeoutTransaction.ResponseCode = cancelResponse.ResponseCode;
                                timeoutTransaction.ResponseMessage = cancelResponse.ResponseMessage;
                                timeoutTransaction.OrderId = cancelResponse.OrderNumber;
                            }
                            else
                            {
                                timeoutTransaction.ErrorCode = cancelResponse.ResponseCode;
                                timeoutTransaction.ErrorMessage = cancelResponse.ResponseMessage;
                                await Retry(timeoutTransaction);
                            }
                        }
                    }
                }
                else if (posResponse.OrderStatus == OrderStatus.Unknown)
                {
                    timeoutTransaction.TimeoutTransactionStatus = oldTimeOutStatus;
                    timeoutTransaction.ResponseCode = posResponse.ResponseCode;
                    timeoutTransaction.ResponseMessage = posResponse.ResponseMessage;
                    await Retry(timeoutTransaction);
                }
            }
            else
            {
                timeoutTransaction.PosErrorCode = posResponse.ResponseCode;
                timeoutTransaction.PosErrorMessage = posResponse.ResponseMessage;
                await Retry(timeoutTransaction);
            }

            if (timeoutTransaction.RetryCount <= 5)
            {
                await _timeoutTransactionRepository.UpdateAsync(timeoutTransaction);
            }
        }
        catch (Exception exception)
        {
            var timeoutException = $"Timeout Transaction Request Exception: {exception} Inner Exception: {exception.InnerException?.Message}";
            _logger.LogError(timeoutException);
            await Retry(timeoutTransaction);
            await _timeoutTransactionRepository.UpdateAsync(timeoutTransaction);
        }
    }

    private async Task Retry(TimeoutTransaction timeoutTransaction)
    {
        try
        {
            timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Pending;
            timeoutTransaction.RetryCount += 1;
            switch (timeoutTransaction.RetryCount)
            {
                case 1:
                    {
                        timeoutTransaction.NextTryTime = DateTime.Now.AddMinutes(5);
                        break;
                    }
                case 2:
                    {
                        timeoutTransaction.NextTryTime = DateTime.Now.AddHours(1);
                        break;
                    }
                case 3:
                    {
                        timeoutTransaction.NextTryTime = DateTime.Now.AddHours(3);
                        break;
                    }
                case 4:
                    {
                        timeoutTransaction.NextTryTime = DateTime.Now.AddHours(12);
                        break;
                    }
                case 5:
                    {
                        timeoutTransaction.NextTryTime = DateTime.Now.AddHours(24);
                        break;
                    }
            }
            if (timeoutTransaction.RetryCount > 5)
            {
                timeoutTransaction.TimeoutTransactionStatus = TimeoutTransactionStatus.Fail;
                await _timeoutTransactionRepository.UpdateAsync(timeoutTransaction);
                var batchSummary = $"Timeout Transaction Batch Failed Running {timeoutTransaction.RetryCount} Times.";
                _logger.LogCritical(batchSummary);
                return;
            }
        }
        catch (Exception exception)
        {
            _logger.LogCritical($"Timeout Transaction Failed to Retry: {exception}");
        }
    }
    private async Task<PosRefundResponse> PosRefund(IVposApi bankService, TimeoutTransaction timeoutTransaction, Merchant subMerchantInfo, MerchantTransaction referenceMerchantTransaction, BankTransaction referenceBankTransaction, MerchantVpos merchantVpos)
    {
        if (referenceBankTransaction is null)
        {
            return new PosRefundResponse
            {
                IsSuccess = false,
                ResponseCode = "99",
                ResponseMessage = "BankTransactionNotFound"
            };
        }

        var originalAuthProcess = await _merchantTransactionRepository.GetAll()
                            .Where(s => s.ConversationId == timeoutTransaction.ConversationId)
                            .OrderBy(s => s.TransactionStartDate)
                            .FirstOrDefaultAsync();

        var currency = await _currencyService.GetByNumberAsync(timeoutTransaction.Currency);
        var merchantNumber = await _merchantRepository.GetAll().Where(s => s.Id == timeoutTransaction.MerchantId).Select(s => s.Number).FirstOrDefaultAsync();

        if (referenceMerchantTransaction.IsOnUsPayment)
        {
            return await _onUsPaymentService.ReturnOnUsPayment(new ReturnCommand
            {
                Amount = 1,
                ConversationId = timeoutTransaction.ConversationId,
                ClientIpAddress = timeoutTransaction.ClientIpAddress
            }, referenceMerchantTransaction, currency);
        }

        return await bankService.Refund(new PosRefundRequest
        {
            Currency = timeoutTransaction.Currency,
            CurrencyCode = currency.Code,
            OrderNumber = await _orderNumberGeneratorService.GenerateForBankTransactionAsync(timeoutTransaction.AcquireBankCode,merchantNumber),
            OrgOrderNumber = timeoutTransaction.OriginalOrderId,
            LanguageCode = timeoutTransaction.LanguageCode,
            Amount = timeoutTransaction.Amount,
            SubMerchantCode = timeoutTransaction.SubMerchantCode,
            CardBrand = CardHelper.GetCardBrand(timeoutTransaction.CardNumber),
            SubMerchantId = subMerchantInfo.Number,
            SubMerchantName = subMerchantInfo.Name,
            SubMerchantCity = subMerchantInfo.Customer.CityName,
            SubMerchantCountry = subMerchantInfo.Customer.Country.ToString(),
            SubMerchantTaxNumber = subMerchantInfo.Customer.TaxNumber,
            SubMerchantMcc = subMerchantInfo.MccCode,
            SubMerchantUrl = subMerchantInfo.WebSiteUrl,
            SubMerchantGlobalMerchantId = subMerchantInfo.GlobalMerchantId,
            SubMerchantPostalCode = subMerchantInfo.Customer.PostalCode,
            ClientIp = timeoutTransaction.ClientIpAddress,
            TotalAmount = referenceBankTransaction.Amount,
            BankOrderId = referenceBankTransaction.BankOrderId,
            ProvisionNumber = referenceBankTransaction.ApprovalCode,
            RRN = referenceBankTransaction.RrnNumber,
            Stan = referenceBankTransaction.Stan,
            OrderDate = timeoutTransaction.TransactionDate,
            OrgAuthProcessOrderNo = originalAuthProcess.OrderId,
            IsTopUpPayment = referenceMerchantTransaction.IsTopUpPayment,
            ServiceProviderPspMerchantId = merchantVpos.ServiceProviderPspMerchantId
        });
    }
    private async Task<PosVoidResponse> PosVoid(IVposApi bankService, TimeoutTransaction timeoutTransaction, Merchant subMerchantInfo, BankTransaction referenceBankTransaction, MerchantVpos merchantVpos)
    {
        if (referenceBankTransaction is null)
        {
            return new PosVoidResponse
            {
                IsSuccess = false,
                ResponseCode = "99",
                ResponseMessage = "BankTransactionNotFound"
            };
        }

        var referenceMerchantTransaction = await _merchantTransactionRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.Id == referenceBankTransaction.MerchantTransactionId);

        var originalAuthProcess = await _merchantTransactionRepository.GetAll()
                           .Where(s => s.ConversationId == timeoutTransaction.ConversationId)
                           .OrderBy(s => s.TransactionStartDate)
                           .FirstOrDefaultAsync();
        
        var merchantNumber = await _merchantRepository.GetAll().Where(s => s.Id == timeoutTransaction.MerchantId).Select(s => s.Number).FirstOrDefaultAsync();
        
        if (referenceMerchantTransaction.IsOnUsPayment)
        {
            return await _onUsPaymentService.ReverseOnUsPayment(referenceMerchantTransaction);
        }

        var currency = await _currencyService.GetByNumberAsync(timeoutTransaction.Currency);

        return await bankService.Void(new PosVoidRequest
        {
            Currency = timeoutTransaction.Currency,
            CurrencyCode = currency.Code,
            OrderNumber = await _orderNumberGeneratorService.GenerateForBankTransactionAsync(timeoutTransaction.AcquireBankCode, merchantNumber),
            OrgOrderNumber = timeoutTransaction.OriginalOrderId,
            LanguageCode = timeoutTransaction.LanguageCode,
            Amount = timeoutTransaction.Amount,
            SubMerchantId = subMerchantInfo.Number,
            SubMerchantName = subMerchantInfo.Name,
            SubMerchantCity = subMerchantInfo.Customer.CityName,
            SubMerchantTaxNumber = subMerchantInfo.Customer.TaxNumber,
            SubMerchantCountry = subMerchantInfo.Customer.Country.ToString(),
            SubMerchantMcc = subMerchantInfo.MccCode,
            SubMerchantUrl = subMerchantInfo.WebSiteUrl,
            SubMerchantGlobalMerchantId = subMerchantInfo.GlobalMerchantId,
            SubMerchantPostalCode = subMerchantInfo.Customer.PostalCode,
            SubMerchantCode = timeoutTransaction.SubMerchantCode,
            ClientIp = timeoutTransaction.ClientIpAddress,
            BankOrderId = referenceBankTransaction.BankOrderId,
            ProvisionNumber = referenceBankTransaction.ApprovalCode,
            RRN = referenceBankTransaction.RrnNumber,
            Stan = referenceBankTransaction.Stan,
            TransactionType = referenceMerchantTransaction.TransactionType == TransactionType.PreAuth && referenceMerchantTransaction.TransactionStatus != TransactionStatus.Closed ? TransactionType.PreAuth : TransactionType.Auth,
            ReverseType = referenceMerchantTransaction.TransactionType,
            OrderDate = referenceMerchantTransaction.TransactionDate,
            OrgAuthProcessOrderNo = originalAuthProcess.OrderId,
            IsTopUpPayment = referenceMerchantTransaction.IsTopUpPayment,
            ServiceProviderPspMerchantId = merchantVpos.ServiceProviderPspMerchantId
        });
    }
    private async Task<Merchant> GetSubMerchant(Guid merchantId)
    {
        var subMerchant = await _merchantRepository.GetAll()
                                                   .Where(b => b.Id == merchantId)
                                                   .Include(b => b.Customer)
                                                   .FirstOrDefaultAsync();

        return subMerchant ?? new Merchant();
    }

}
