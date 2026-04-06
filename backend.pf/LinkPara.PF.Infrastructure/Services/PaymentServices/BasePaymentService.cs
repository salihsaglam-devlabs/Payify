using LinkPara.HttpProviders.Calendar;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Limit;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.MerchantTransactions.Command;
using LinkPara.PF.Application.Features.MerchantTransactions.Command.GenerateOrderNumber;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using LinkPara.HttpProviders.Vault;
using TransactionStatus = LinkPara.PF.Domain.Enums.TransactionStatus;

namespace LinkPara.PF.Infrastructure.Services.PaymentServices;

public class BasePaymentService : IBasePaymentService
{
    private const int InstallmentTwentyFour = 24;
    private const int InstallmentThirtySix = 36;
    
    private readonly IGenericRepository<PricingProfile> _pricingProfileRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly ILimitService _limitService;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IOrderNumberGeneratorService _orderNumberGeneratorService;
    private readonly ICalendarService _calendarService;
    private readonly IGenericRepository<TimeoutTransaction> _timeoutTransactionRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly PfDbContext _dbContext;
    private readonly IBus _bus;
    private readonly IVaultClient _vaultClient;
    
    public BasePaymentService(
        IGenericRepository<PricingProfile> pricingProfileRepository, 
        ILimitService limitService,
        IResponseCodeService errorCodeService,
        IOrderNumberGeneratorService orderNumberGeneratorService,
        ICalendarService calendarService,
        IGenericRepository<TimeoutTransaction> timeoutTransactionRepository,
        IApplicationUserService applicationUserService,
        PfDbContext dbContext,
        IBus bus, 
        IVaultClient vaultClient, 
        IGenericRepository<AcquireBank> acquireBankRepository)
    {
        _pricingProfileRepository = pricingProfileRepository;
        _limitService = limitService;
        _errorCodeService = errorCodeService;
        _orderNumberGeneratorService = orderNumberGeneratorService;
        _calendarService = calendarService;
        _timeoutTransactionRepository = timeoutTransactionRepository;
        _applicationUserService = applicationUserService;
        _dbContext = dbContext;
        _bus = bus;
        _vaultClient = vaultClient;
        _acquireBankRepository = acquireBankRepository;
    }
    
    public PricingProfile GetPricingProfileByTransaction(MerchantDto merchant, Currency currency)
    {
        return _pricingProfileRepository.GetAll()
            .Include(p => p.Currency)
            .Include(i => i.PricingProfileItems).ThenInclude(b => b.PricingProfileInstallments)
            .SingleOrDefault(w => 
                w.PricingProfileNumber == merchant.PricingProfileNumber && 
                w.ProfileStatus == ProfileStatus.InUse &&
                w.CurrencyCode == currency.Code);
    }
    
    public async Task<ValidationResponse> CheckLimitControlAsync(ProvisionCommand request, VposPaymentType paymentType)
    {
        var transactionType = paymentType switch
        {
            VposPaymentType.Auth => TransactionType.Auth,
            VposPaymentType.PreAuth => TransactionType.PreAuth,
            VposPaymentType.PostAuth => TransactionType.Return,
            _ => TransactionType.Auth
        };

        return await _limitService.CheckLimitAsync(new CheckLimitRequest
        {
            ConversationId = request.ConversationId,
            Currency = request.Currency,
            Amount = request.Amount,
            CardToken = request.CardToken,
            ClientIpAddress = request.ClientIpAddress,
            InstallmentCount = request.InstallmentCount,
            LanguageCode = request.LanguageCode,
            MerchantId = request.MerchantId,
            SubMerchantId = request.SubMerchantId ?? null,
            PointAmount = request.PointAmount,
            ThreeDSessionId = request.ThreeDSessionId,
            TransactionType = transactionType
        });
    }
    
    public async Task<ProvisionResponse> GetProvisionResponseAsync(string code, ProvisionCommand request)
    {
        var apiResponse = await _errorCodeService.GetApiResponseCode(code, request.LanguageCode);

        return new ProvisionResponse
        {
            IsSucceed = false,
            ErrorCode = apiResponse.ResponseCode,
            ConversationId = request.ConversationId,
            ErrorMessage = apiResponse.DisplayMessage,
            OrderId = request.OriginalOrderId
        };
    }
    
    public async Task<string> GenerateOrderNumberAsync(Guid merchantId, string orderNumber)
    {
        OrderNumberResponse orderNumberResponse;
        do
        {
            orderNumberResponse = await _orderNumberGeneratorService.GenerateAsync(new GenerateOrderNumberCommand()
            {
                MerchantId = merchantId,
                OrderNumber = orderNumber
            });
        } while (!orderNumberResponse.IsSuccess);

        return orderNumberResponse.OrderNumber;
    }
    
    public PricingProfileItem GetPricingProfileItemByTransaction(PricingProfile pricingProfile, MerchantTransaction merchantTransaction)
    {
        var profileCardType = GetProfileCardType(merchantTransaction);

        var pricingProfileItems = pricingProfile
            .PricingProfileItems
            .Where(w => w.PricingProfileId == pricingProfile.Id
                        && w.ProfileCardType == profileCardType
                        && w.IsActive).ToList();
        PricingProfileItem pricingProfileItem = null;

        if (merchantTransaction.InstallmentCount <= 12)
        {
            pricingProfileItem = pricingProfileItems
                .FirstOrDefault(s => s.InstallmentNumber == merchantTransaction.InstallmentCount);
        }
        else
        {
            int endInstallmentNumber = (merchantTransaction.InstallmentCount <= 24) ? InstallmentTwentyFour : InstallmentThirtySix;
            pricingProfileItem = pricingProfileItems
                .FirstOrDefault(s => s.InstallmentNumberEnd == endInstallmentNumber);
        }

        return pricingProfileItem;
    }
    
    private static ProfileCardType GetProfileCardType(MerchantTransaction merchantTransaction)
    {
        ProfileCardType profileCardType;

        if (merchantTransaction.IsOnUsPayment)
            profileCardType = ProfileCardType.Wallet;
        else if (merchantTransaction.IsAmex)
            profileCardType = ProfileCardType.Amex;
        else if (merchantTransaction.IsInternational)
            profileCardType = ProfileCardType.International;
        else if (merchantTransaction.CardType is CardType.Credit or CardType.Unknown)
            profileCardType = ProfileCardType.Credit;
        else
            profileCardType = ProfileCardType.Debit;

        return profileCardType;
    }
    
    public async Task<DateTime> CalculatePaymentDateAsync(DateTime transactionDate, int dueDate)
    {
        try
        {
            var dateWithDue = transactionDate.AddDays(dueDate);

            return dateWithDue;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
    
    public async Task<bool> GetIsPaymentDateWillBeShiftedAsync(DateTime transactionEndDate, int acquireBankCode)
    {
        bool shiftPaymentDateParam;
        try
        {
            shiftPaymentDateParam = await _vaultClient.GetSecretValueAsync<bool>("PFSecrets", "TransactionSettings", "ShiftPaymentDateAfterBankEOD");
        }
        catch (Exception)
        {
            shiftPaymentDateParam = false;
        }
        
        var acquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(s => s.BankCode == acquireBankCode);
        
        if (!shiftPaymentDateParam || acquireBank is null)
        {
            return false;
        }

        var today = DateTime.Today;
        var bankEndOfDay =  new DateTime(
            today.Year,
            today.Month,
            today.Day,
            acquireBank.EndOfDayHour,
            acquireBank.EndOfDayMinute,
            0,
            DateTimeKind.Local
        );

        return transactionEndDate >= bankEndOfDay;
    }
    
    public async Task InsertTimeoutTransactionAsync(MerchantTransaction merchantTransaction,
        BankTransaction bankTransaction,
        string clientIpAddress,
        string originalOrderId = null
    )
    {
        await FailedTransactionUpdateAsync(merchantTransaction, bankTransaction);

        await _timeoutTransactionRepository.AddAsync(new TimeoutTransaction
        {
            MerchantId = merchantTransaction.MerchantId,
            ConversationId = merchantTransaction.ConversationId,
            TransactionType = merchantTransaction.TransactionType,
            TimeoutTransactionStatus = TimeoutTransactionStatus.Pending,
            TransactionDate = DateTime.Now,
            VposId = merchantTransaction.VposId,
            MerchantTransactionId = merchantTransaction.Id,
            BankTransactionId = bankTransaction.Id,
            AcquireBankCode = merchantTransaction.AcquireBankCode,
            OriginalOrderId = string.IsNullOrEmpty(originalOrderId)
                ? bankTransaction.OrderId
                : originalOrderId,
            OrderId = bankTransaction.OrderId,
            SubMerchantCode = bankTransaction.SubMerchantCode,
            Currency = merchantTransaction.Currency,
            LanguageCode = merchantTransaction.LanguageCode,
            Amount = bankTransaction.Amount,
            CardNumber = bankTransaction.CardNumber,
            CreateDate = DateTime.Now,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            RecordStatus = RecordStatus.Active,
            Description = "Transaction Timeout",
            ClientIpAddress = clientIpAddress
        });
    }
    
    private async Task FailedTransactionUpdateAsync(MerchantTransaction merchantTransaction,
        BankTransaction bankTransaction)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            merchantTransaction.ResponseCode = "99";
            merchantTransaction.ResponseDescription = "TimeoutException";
            merchantTransaction.TransactionEndDate = DateTime.Now;
            merchantTransaction.TransactionStatus = TransactionStatus.Fail;

            bankTransaction.BankResponseCode = "99";
            bankTransaction.BankResponseDescription = "TimeOutException";
            bankTransaction.TransactionEndDate = DateTime.Now;
            bankTransaction.TransactionStatus = TransactionStatus.Fail;
            bankTransaction.BankTransactionDate = DateTime.Now;

            _dbContext.MerchantTransaction.Update(merchantTransaction);

            await _dbContext.AddAsync(bankTransaction);

            await _dbContext.SaveChangesAsync();
            transactionScope.Complete();
        });
    }
    
    public async Task PublishIncrementLimitAsync(Guid merchantTransactionId)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.IncrementLimits"));
        await endpoint.Send(new IncrementLimits
        {
            MerchantTransactionId = merchantTransactionId
        }, tokenSource.Token);
    }
    
    public async Task<ValidationResponse> GetValidationResponseAsync(string errorCode, string languageCode)
    {
        var merchantResponse = await _errorCodeService.GetApiResponseCode(errorCode, languageCode);

        return new ValidationResponse
        {
            Code = merchantResponse.ResponseCode,
            IsValid = false,
            Message = merchantResponse.DisplayMessage
        };
    }
}