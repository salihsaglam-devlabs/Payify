using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Calendar;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionSource = LinkPara.SharedModels.Banking.Enums.TransactionSource;

namespace LinkPara.PF.Application.Features.PostingBalances.Commands.RetryPayment;

public class RetryPaymentCommand : IRequest
{
    public Guid BalanceId { get; set; }
}

public class RetryPaymentCommandHandler : IRequestHandler<RetryPaymentCommand>
{
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
    private readonly IBus _bus;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly ILogger<RetryPaymentCommandHandler> _logger;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ICalendarService _calendarService;
    private readonly IParameterService _parameterService;

    private int moneyTransferStartHour;
    private static string Tenant => Environment.GetEnvironmentVariable("Tenant");

    public RetryPaymentCommandHandler(IGenericRepository<Merchant> merchantRepository,
        IBus bus,
        IGenericRepository<PostingBalance> postingBalanceRepository,
        ILogger<RetryPaymentCommandHandler> logger,
        IApplicationUserService applicationUserService,
        ICalendarService calendarService,
        IParameterService parameterService)
    {
        _merchantRepository = merchantRepository;
        _bus = bus;
        _postingBalanceRepository = postingBalanceRepository;
        _logger = logger;
        _applicationUserService = applicationUserService;
        _calendarService = calendarService;
        _parameterService = parameterService;
    }

    public async Task<Unit> Handle(RetryPaymentCommand request, CancellationToken cancellationToken)
    {
        var balance = await _postingBalanceRepository.GetAll()
            .Where(s => s.Id == request.BalanceId &&
                        (s.MoneyTransferStatus == PostingMoneyTransferStatus.PaymentError ||
                         s.MoneyTransferStatus == PostingMoneyTransferStatus.PaymentReturned))
            .FirstOrDefaultAsync(cancellationToken);

        if (balance == null)
        {
            throw new NotFoundException(nameof(PostingBalance), request.BalanceId);
        }

        var merchant = await _merchantRepository.GetAll()
            .Include(s => s.MerchantBankAccounts)
            .Include(s => s.MerchantWallets)
            .Where(q => q.Id == balance.MerchantId
                        && q.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync(cancellationToken);

        if (merchant == null)
        {
            throw new NotFoundException(nameof(Merchant), balance.MerchantId);
        }

        if (!merchant.PaymentAllowed)
        {
            throw new PaymentIsNotAllowedForMerchantException();
        }

        var referenceId = Guid.NewGuid();
        var totalBalance = balance.TotalPayingAmount;
        if (totalBalance <= 0)
        {
            balance.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentWaiting;
            await _postingBalanceRepository.UpdateAsync(balance);
            throw new PayingTotalAmountIsLessZeroException();
        }

        var merchantBankAccount = merchant.MerchantBankAccounts
            .FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);

        var merchantWallet = merchant.MerchantWallets
            .FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);

        var defaultTransferHour = await MoneyTransferHourHelper.GetMoneyTransferHourAsync(_parameterService, _logger);
        var holidayTransferAmountThreshold =
            await MoneyTransferHourHelper.GetMoneyHolidayTransferAmountThresholdAsync(_parameterService, _logger);
        var workingDate = DateTime.Now.Date;
        if (
            (
                workingDate.DayOfWeek == DayOfWeek.Saturday ||
                workingDate.DayOfWeek == DayOfWeek.Sunday ||
                await _calendarService.IsHolidayAsync(workingDate, "TUR")
            )
            && totalBalance > holidayTransferAmountThreshold)
        {
            workingDate = (await _calendarService.NextWorkDayAsync(workingDate, "TUR")).Date;
        }

        workingDate = workingDate
            .AddHours(merchant.MoneyTransferStartHour ?? defaultTransferHour.Hours)
            .AddMinutes(merchant.MoneyTransferStartMinute ?? defaultTransferHour.Minutes);

        var transferRequest = new SaveTransferRequest
        {
            UserId = _applicationUserService.ApplicationUserId,
            Amount = totalBalance,
            CurrencyCode = "TRY",
            ReceiverName = merchant.Name,
            Source = TransactionSource.PF,
            TransactionSourceReferenceId = referenceId,
            IsReturnPayment = false,
            Description = $"{Tenant} - {workingDate.Date}",
            WorkingDate = workingDate
        };

        if (merchant.PostingPaymentChannel == PostingPaymentChannel.Wallet)
        {
            if (merchantWallet == null)
            {
                _logger.LogError("MerchantWalletNotFoundForMerchant: {MerchantId}", balance.MerchantId);

                balance.BatchStatus = BatchStatus.MoneyTransferError;
                await _postingBalanceRepository.UpdateAsync(balance);

                throw new NotFoundException(nameof(MerchantWallet), balance.MerchantId);
            }

            transferRequest.WalletNumber = merchantWallet.WalletNumber;
            balance.WalletNumber = merchantWallet.WalletNumber;
        }
        else
        {
            if (merchantBankAccount == null)
            {
                _logger.LogError("MerchantBankAccountNotFoundForMerchant: {MerchantId}", balance.MerchantId);

                balance.BatchStatus = BatchStatus.MoneyTransferError;
                await _postingBalanceRepository.UpdateAsync(balance);

                throw new NotFoundException(nameof(MerchantBankAccount), balance.MerchantId);
            }

            transferRequest.ReceiverIBAN = merchantBankAccount.Iban;
            balance.Iban = merchantBankAccount.Iban;
        }

        balance.BatchStatus = BatchStatus.Completed;
        balance.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentInitiated;
        balance.MoneyTransferPaymentDate = DateTime.Now;
        balance.TransactionSourceId = referenceId;
        balance.PostingPaymentChannel = merchant.PostingPaymentChannel;

        await _postingBalanceRepository.UpdateAsync(balance);

        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:MoneyTransfer.SaveTransfer"));
        await endpoint.Send(transferRequest, cancellationToken);
        return Unit.Value;
    }
}