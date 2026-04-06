using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Application.Features.Payments.Commands.Return;
using LinkPara.PF.Application.Features.Payments.Commands.VerifyOnUsPayment;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Transactions;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using EmoneyProvisionResponse = LinkPara.HttpProviders.Emoney.Models.ProvisionResponse;
using IFraudService = LinkPara.PF.Application.Commons.Interfaces.IFraudService;
using IOnUsPaymentService = LinkPara.PF.Application.Commons.Interfaces.IOnUsPaymentService;
using ProvisionResponse = LinkPara.PF.Application.Commons.Models.Payments.Response.ProvisionResponse;
using TransactionStatus = LinkPara.PF.Domain.Enums.TransactionStatus;

namespace LinkPara.PF.Infrastructure.Services.PaymentServices;

public class OnUsPaymentService : IOnUsPaymentService
{
    private const string GenericErrorCode = "99";
    private const string GenericSuccessCode = "00";
    private const string EmoneyInquireNotFoundErrorCode = "103";
    private const string EmoneyUserRejectedMessage = "User Rejected";
    
    private readonly IBasePaymentService _basePaymentService;
    private readonly ICurrencyService _currencyService;
    private readonly IPosRouterService _posRouterService;
    private readonly IFraudService _fraudService;
    private readonly IContextProvider _contextProvider;
    private readonly IParameterService _parameterService;
    private readonly PfDbContext _dbContext;
    private readonly IResponseCodeService _errorCodeService;
    private readonly HttpProviders.Emoney.IOnUsPaymentService _emoneyOnUsPaymentService;
    private readonly IBus _bus;
    private readonly ILogger<OnUsPaymentService> _logger;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IProvisionService _emoneyProvisionService;
    private readonly IGenericRepository<OnUsPayment> _onUsPaymentRepository;
    private readonly IServiceScopeFactory _scopeFactory;

    public OnUsPaymentService(
        IBasePaymentService basePaymentService,
        ICurrencyService currencyService,
        IPosRouterService posRouterService,
        IFraudService fraudService,
        IContextProvider contextProvider,
        IParameterService parameterService,
        PfDbContext dbContext,
        IResponseCodeService errorCodeService,
        HttpProviders.Emoney.IOnUsPaymentService emoneyOnUsPaymentService,
        IBus bus,
        ILogger<OnUsPaymentService> logger,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IProvisionService emoneyProvisionService,
        IGenericRepository<OnUsPayment> onUsPaymentRepository,
        IServiceScopeFactory scopeFactory)
    {
        _basePaymentService = basePaymentService;
        _currencyService = currencyService;
        _posRouterService = posRouterService;
        _fraudService = fraudService;
        _contextProvider = contextProvider;
        _parameterService = parameterService;
        _dbContext = dbContext;
        _errorCodeService = errorCodeService;
        _emoneyOnUsPaymentService = emoneyOnUsPaymentService;
        _bus = bus;
        _logger = logger;
        _merchantTransactionRepository = merchantTransactionRepository;
        _emoneyProvisionService = emoneyProvisionService;
        _onUsPaymentRepository = onUsPaymentRepository;
        _scopeFactory = scopeFactory;
    }
    
    public async Task<ValidationResponse> PreValidateOnUsAuth(ProvisionCommand request, MerchantDto merchant, bool parentMerchantFinancialTransaction, SubMerchantDto subMerchant)
    {
        if (merchant is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchant,request.LanguageCode);
        }

        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus,request.LanguageCode);
        }

        if (!merchant.IntegrationMode.ToString().Contains(IntegrationMode.OnUs.ToString()))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.IntegrationModeNotAllowed,request.LanguageCode);
        }

        if (request.InstallmentCount > 1)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InstallmentNotAllowed,request.LanguageCode);
        }

        if (!string.IsNullOrEmpty(subMerchant.Name))
        {
            if (request.IntegrationMode == IntegrationMode.OnUs && !subMerchant.IsOnUsPaymentPageAllowed)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.SubMerchantIntegrationModeNotAllowed, request.LanguageCode);
            }

            if (request.InstallmentCount > 1 && !subMerchant.InstallmentAllowed)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.SubMerchantInstallmentNotAllowed, request.LanguageCode);
            }

            if (subMerchant.RecordStatus != RecordStatus.Active)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidSubMerchantStatus, request.LanguageCode);
            }
        }

        var currency = await _currencyService.GetByCodeAsync(request.Currency);

        var pricingProfile = _basePaymentService.GetPricingProfileByTransaction(merchant, currency);

        if (pricingProfile is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.PricingProfileItemNotFound,request.LanguageCode);
        }

        if (request.PaymentType != VposPaymentType.Auth)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidPaymentType,request.LanguageCode);
        }
        
        if (request.CallbackUrl is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CallbackUrlRequired,request.LanguageCode);
        }

        if (string.IsNullOrEmpty(request.MerchantCustomerPhoneCode) ||
            string.IsNullOrEmpty(request.MerchantCustomerPhoneNumber))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CustomerPhoneNumberRequired,request.LanguageCode);
        }

        var checkLimit = await _basePaymentService.CheckLimitControlAsync(request, request.PaymentType);

        return !checkLimit.IsValid ? checkLimit : new ValidationResponse { IsValid = true };
    }

    public async Task<ProvisionResponse> InitiateOnUsPaymentAsync(ProvisionCommand request, MerchantDto merchant,
        MerchantTransaction merchantTransaction,
        string parseUserId)
    {
        try
        {
            var routeInfo = await _posRouterService.OnUsRouteAsync(merchant, request.Currency, request.Amount);

            var currency = await _currencyService.GetByCodeAsync(request.Currency);
            
            if (!await _fraudService.CheckFraudAsync(new FraudTransactionDetail
                {
                    Amount = request.Amount,
                    BeneficiaryNumber = merchant.Number,
                    Beneficiary = merchant.Name,
                    BeneficiaryBankID = routeInfo.AcquireBank.BankCode.ToString(),
                    OriginatorNumber = request.MerchantCustomerPhoneNumber,
                    Originator = string.Empty,
                    OriginatorBankID = routeInfo.AcquireBank.BankCode.ToString(),
                    FraudSource = FraudSource.Pos,
                    Direction = Direction.Outbound,
                    AmountCurrencyCode = currency.Number,
                    BeneficiaryAccountCurrencyCode = currency.Number,
                    OriginatorAccountCurrencyCode = currency.Number,
                    Channel = _contextProvider.CurrentContext.Channel,
                    TransactionType = request.PaymentType.ToString(),
                    MccCode = Convert.ToInt32(merchant.MccCode)
                },"PfProvision",request.ClientIpAddress))
            {
                return  await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.PotentialFraud, request);
            }

            merchantTransaction.Currency = currency.Number;
            merchantTransaction.VposId = routeInfo.Vpos.Id;
            merchantTransaction.VposName = routeInfo.Vpos.Name;
            merchantTransaction.IssuerBankCode = routeInfo.AcquireBank.BankCode;
            merchantTransaction.AcquireBankCode = routeInfo.AcquireBank.BankCode;
            merchantTransaction.BankPaymentDate = DateTime.Now.AddDays(routeInfo.BlockedDayNumber + 1);
            merchantTransaction.CardTransactionType = CardTransactionType.OnUs;
            merchantTransaction.CardType = CardType.Debit;
            merchantTransaction.OrderId = await _basePaymentService.GenerateOrderNumberAsync(merchant.Id, null);
            merchantTransaction.LastModifiedBy = parseUserId;
            merchantTransaction.Description = request.Description;
            merchantTransaction.BankCommissionAmount = routeInfo.CommissionAmount;
            merchantTransaction.BankCommissionRate = routeInfo.CommissionRate;
            merchantTransaction.IsOnUsPayment = true;

            var pricingProfile = _basePaymentService.GetPricingProfileByTransaction(merchant, currency);
            var pricingProfileItem = _basePaymentService.GetPricingProfileItemByTransaction(pricingProfile, merchantTransaction);
            if (pricingProfileItem is null)
            {
                return await _basePaymentService.GetProvisionResponseAsync(ApiErrorCode.PricingProfileItemNotFound, request);
            }

            merchantTransaction.PfCommissionAmount = pricingProfile.PerTransactionFee +
                                                     pricingProfileItem.CommissionRate / 100m *
                                                     merchantTransaction.Amount;
            merchantTransaction.PfNetCommissionAmount =
                merchantTransaction.PfCommissionAmount - merchantTransaction.BankCommissionAmount;
            merchantTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;
            merchantTransaction.PfPerTransactionFee = pricingProfile.PerTransactionFee;
            merchantTransaction.ParentMerchantCommissionRate = pricingProfileItem.ParentMerchantCommissionRate;
            merchantTransaction.ParentMerchantCommissionAmount = pricingProfileItem.ParentMerchantCommissionRate / 100m * merchantTransaction.Amount;
            merchantTransaction.AmountWithoutCommissions =
                merchantTransaction.Amount - merchantTransaction.PfCommissionAmount - merchantTransaction.ParentMerchantCommissionAmount;
            merchantTransaction.AmountWithoutBankCommission =
                merchantTransaction.Amount - merchantTransaction.BankCommissionAmount;
            merchantTransaction.AmountWithoutParentMerchantCommission =
                merchantTransaction.Amount - merchantTransaction.ParentMerchantCommissionAmount;
            merchantTransaction.PricingProfileItemId = pricingProfileItem.Id;
            merchantTransaction.BsmvAmount =
                await BsmvAmountCalculateHelper.CalculateBsmvAmount(merchantTransaction.PfNetCommissionAmount,
                    _parameterService);
            merchantTransaction.PfPaymentDate = await _basePaymentService.CalculatePaymentDateAsync(merchantTransaction.TransactionDate,
                pricingProfileItem.BlockedDayNumber);
            merchantTransaction.PointCommissionRate = 0;
            merchantTransaction.PointCommissionAmount = 0;

            var expireInMinutes = await _parameterService.GetParameterAsync("PFPaymentParameters", "OnUsExpireInMinutes");

            var onUsPayment = new OnUsPayment
            {
                Status = ChannelStatus.Active,
                PaymentStatus = ChannelPaymentStatus.Pending,
                WebhookStatus = WebhookStatus.Pending,
                MerchantId = merchantTransaction.MerchantId,
                MerchantTransactionId = merchantTransaction.Id,
                MerchantName = merchant.Name,
                MerchantNumber = merchant.Number,
                PhoneCode = merchantTransaction.MerchantCustomerPhoneCode,
                PhoneNumber = merchantTransaction.MerchantCustomerPhoneNumber,
                ExpiryDate = DateTime.Now.AddMinutes(Convert.ToInt32(expireInMinutes.ParameterValue)),
                WebhookRetryCount = 0,
                CallbackUrl = request.CallbackUrl,
                OrderId = merchantTransaction.OrderId,
                Amount = merchantTransaction.Amount,
                Currency = merchantTransaction.Currency,
                EmoneyTransactionId = Guid.Empty
            };

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                
                await _dbContext.OnUsPayment.AddAsync(onUsPayment);
                _dbContext.MerchantTransaction.Update(merchantTransaction);

                await _dbContext.SaveChangesAsync();
                scope.Complete();
            });
            
            var emoneyInitResponse = await _emoneyOnUsPaymentService.InitOnUsPaymentAsync(new InitOnUsPaymentRequest
            {
                Amount = onUsPayment.Amount,
                Currency = currency.Code,
                PhoneCode = onUsPayment.PhoneCode,
                PhoneNumber = onUsPayment.PhoneNumber,
                ConversationId = merchantTransaction.ConversationId,
                OrderId = merchantTransaction.OrderId,
                MerchantName = merchant.Name,
                MerchantNumber = merchant.Number,
                ExpireDate = onUsPayment.ExpiryDate,
                RequestDate = merchantTransaction.TransactionStartDate
            });

            if (emoneyInitResponse.IsSuccess)
            {
                return new ProvisionResponse
                {
                    IsSucceed = true,
                    ConversationId = merchantTransaction.ConversationId,
                    OrderId = merchantTransaction.OrderId,
                    Description = "PaymentInitiated",
                };
            }
            
            merchantTransaction.ResponseCode = emoneyInitResponse.ErrorCode;
            merchantTransaction.ResponseDescription = emoneyInitResponse.ErrorMessage;
            merchantTransaction.TransactionStatus = TransactionStatus.Fail;
            merchantTransaction.TransactionEndDate = DateTime.Now;
            _dbContext.Update(merchantTransaction);
            
            onUsPayment.Status = ChannelStatus.Cancelled;
            onUsPayment.PaymentStatus = ChannelPaymentStatus.Failed;
            _dbContext.OnUsPayment.Update(onUsPayment);
            await _dbContext.SaveChangesAsync();
                
            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = merchantTransaction.ResponseCode,
                ErrorMessage = merchantTransaction.ResponseDescription,
                OrderId = merchantTransaction.OrderId,
                ConversationId = request.ConversationId,
                Description = merchantTransaction.Description,
            };
        }
        catch (Exception exception)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();
            
            merchantTransaction.ResponseCode =
                (exception is ApiException apiException)
                    ? apiException.Code
                    : GenericErrorCode;
            merchantTransaction.ResponseDescription = $"{exception.GetType().Name} - {exception.Message}";
            merchantTransaction.TransactionStatus = TransactionStatus.Fail;
            merchantTransaction.TransactionEndDate = DateTime.Now;
            dbContext.Update(merchantTransaction);
            
            var onUsPayment = await dbContext.OnUsPayment.FirstOrDefaultAsync(s => s.MerchantTransactionId == merchantTransaction.Id);

            if (onUsPayment is not null)
            {
                onUsPayment.Status = ChannelStatus.Cancelled;
                onUsPayment.PaymentStatus = ChannelPaymentStatus.Failed;
                dbContext.Update(onUsPayment);
            }
            await dbContext.SaveChangesAsync();

            var apiResponse = await _errorCodeService.GetApiResponseCode(merchantTransaction.ResponseCode, request.LanguageCode);

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = merchantTransaction.ResponseCode,
                ErrorMessage = apiResponse.DisplayMessage,
                OrderId = string.Empty,
                ConversationId = request.ConversationId,
                Description = merchantTransaction.Description
            };
        }
    }
    
    public async Task TriggerOnUsWebhookAsync(Guid onUsId)
    {
        try
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.OnUsWebhook"));
            await endpoint.Send(new OnUsWebhook { OnUsId = onUsId }, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"TriggerOnUsWebhook Failed ! - Id({onUsId}) Error : {exception}");
        }
    }
    
    public async Task<ProvisionResponse> CompleteOnUsProvisionAsync(VerifyOnUsPaymentCommand request)
    {
        var onUsPayment = await _dbContext.OnUsPayment
            .FirstOrDefaultAsync(s => s.OrderId == request.OrderId);

        var merchant = await _dbContext.Merchant.FirstOrDefaultAsync(s => s.Number == request.MerchantNumber);

        var parentMerchantFinancialTransaction = true;
        if (merchant?.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
        {
            var parentMerchant = await _dbContext.Merchant.FirstOrDefaultAsync(s => s.Id == merchant.ParentMerchantId);
            if (parentMerchant is not null)
            {
                parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
            }
        }


        var currency = await _currencyService.GetByNumberAsync(onUsPayment.Currency);

        var validationResponse = await PreValidateOnUsProvisionAsync(onUsPayment, merchant, parentMerchantFinancialTransaction, request);

        if (!validationResponse.IsValid)
        {
            _logger.LogError($"OnUs PreValidation failed with code : {validationResponse.Code}, " +
                             $"Message: {validationResponse.Message}");

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = validationResponse.Code,
                ConversationId = request.ConversationId,
                ErrorMessage = validationResponse.Message,
                OrderId = request.OrderId
            };
        }

        var merchantTransaction = await _merchantTransactionRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == onUsPayment.MerchantTransactionId);

        var bankTransaction = new BankTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedBy = merchant.Id.ToString(),
            TransactionStartDate = DateTime.Now,
            TransactionStatus = TransactionStatus.Pending,
            TransactionType = TransactionType.Auth,
            Amount = onUsPayment.Amount,
            PointAmount = 0,
            InstallmentCount = 1,
            Is3ds = false,
            IsReverse = false,
            MerchantTransactionId = merchantTransaction.Id,
            OrderId = merchantTransaction.OrderId,
            Currency = currency.Number,
            CardNumber = request.WalletNumber,
            VposId = merchantTransaction.VposId,
            IssuerBankCode = merchantTransaction.AcquireBankCode,
            AcquireBankCode = merchantTransaction.AcquireBankCode,
            MerchantCode = "0",
            SubMerchantCode = "0",
            EndOfDayStatus = EndOfDayStatus.Pending
        };
        
        var emoneyProvisionResponse = new EmoneyProvisionResponse();

        if (!request.IsVerifiedByUser)
        {
            emoneyProvisionResponse.ErrorCode = ApiErrorCode.EmoneyUserRejected;
            emoneyProvisionResponse.ErrorMessage = EmoneyUserRejectedMessage;
            emoneyProvisionResponse.TransactionId = Guid.Empty;
            var provisionResponse = await MarkAsFailedAsync(merchantTransaction, bankTransaction, emoneyProvisionResponse, onUsPayment, request);

            provisionResponse.TransactionId = merchantTransaction.Id;

            await TriggerOnUsWebhookAsync(onUsPayment.Id);

            return provisionResponse;
        }
        
        try
        {
            try
            {
                var provisionRequest = new ProvisionRequest
                {
                    Amount = onUsPayment.Amount,
                    ClientIpAddress = request.ClientIpAddress,
                    ConversationId = merchantTransaction.ConversationId,
                    ProvisionSource = ProvisionSource.Onus,
                    UserId = onUsPayment.MerchantId,
                    WalletNumber = request.WalletNumber,
                    Description = $"OnUsPayment-{request.OrderId}-{request.WalletNumber}",
                    CurrencyCode = currency.Code,
                    Tag = merchant.Name,
                    BsmvAmount = 0,
                    CommissionAmount = 0,
                    OrderId = onUsPayment.OrderId
                };

                emoneyProvisionResponse = await _emoneyProvisionService.ProvisionAsync(provisionRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError($"OnUsEmoneyProvisionException: {exception}");
                
                onUsPayment.Status = ChannelStatus.Cancelled;
                onUsPayment.PaymentStatus = ChannelPaymentStatus.Failed;
                onUsPayment.Name = request.Name;
                onUsPayment.Surname = request.Surname;
                onUsPayment.Email = request.Email;
                onUsPayment.PhoneCode = request.PhoneCode;
                onUsPayment.PhoneNumber = request.PhoneNumber;
                onUsPayment.WalletNumber = request.WalletNumber;
                await _onUsPaymentRepository.UpdateAsync(onUsPayment);
                
                await TriggerOnUsWebhookAsync(onUsPayment.Id);
                
                await _basePaymentService.InsertTimeoutTransactionAsync(merchantTransaction, bankTransaction, request.ClientIpAddress);

                var timeoutExceptionCode = await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.BankHasTimedOut, request.LanguageCode);
                var apiResponse =
                    await _errorCodeService.GetApiResponseCode(timeoutExceptionCode.Code, request.LanguageCode);

                return new ProvisionResponse
                {
                    IsSucceed = false,
                    ErrorCode = apiResponse.ResponseCode,
                    ErrorMessage = apiResponse.DisplayMessage,
                    OrderId = string.Empty,
                    ConversationId = request.ConversationId,
                    TransactionId = merchantTransaction.Id
                };
            }

            var provisionResponse = emoneyProvisionResponse.IsSucceed
                ? await MarkAsCompletedAsync(merchantTransaction, bankTransaction, emoneyProvisionResponse, onUsPayment, request)
                : await MarkAsFailedAsync(merchantTransaction, bankTransaction, emoneyProvisionResponse, onUsPayment, request);

            provisionResponse.TransactionId = merchantTransaction.Id;

            await TriggerOnUsWebhookAsync(onUsPayment.Id);

            return provisionResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError($"OnUsProvision Error - {exception}");

            if (emoneyProvisionResponse is not null)
            {
                return await RetryDbUpdateAsync(merchantTransaction, emoneyProvisionResponse, bankTransaction,
                    onUsPayment, request);
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                merchantTransaction.ResponseCode =
                    (exception is ApiException apiException)
                        ? apiException.Code
                        : GenericErrorCode;
                merchantTransaction.ResponseDescription = $"{exception.GetType().Name} - {exception.Message}";
                merchantTransaction.TransactionStatus = TransactionStatus.Fail;
                merchantTransaction.TransactionEndDate = DateTime.Now;
                _dbContext.Update(merchantTransaction);
                
                onUsPayment.Status = ChannelStatus.Cancelled;
                onUsPayment.PaymentStatus = ChannelPaymentStatus.Failed;
                onUsPayment.Name = request.Name;
                onUsPayment.Surname = request.Surname;
                onUsPayment.Email = request.Email;
                onUsPayment.PhoneCode = request.PhoneCode;
                onUsPayment.PhoneNumber = request.PhoneNumber;
                onUsPayment.WalletNumber = request.WalletNumber;
                _dbContext.Update(onUsPayment);
    
                bankTransaction.TransactionEndDate = DateTime.Now;
                bankTransaction.BankResponseCode = string.Empty;
                bankTransaction.BankResponseDescription = string.Empty;
                bankTransaction.TransactionStatus = TransactionStatus.Fail;
                await _dbContext.AddAsync(bankTransaction);
                
                await _dbContext.SaveChangesAsync();
                
                await TriggerOnUsWebhookAsync(onUsPayment.Id);
                
                scope.Complete();
            });

            var apiResponse = await _errorCodeService.GetApiResponseCode(merchantTransaction.ResponseCode, request.LanguageCode);

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = merchantTransaction.ResponseCode,
                ErrorMessage = apiResponse.DisplayMessage,
                OrderId = string.Empty,
                ConversationId = request.ConversationId
            };
        }
    }

    public async Task<PosPaymentDetailResponse> GetPaymentDetailAsync(string orderId)
    {
        var onUsPayment = await _onUsPaymentRepository.GetAll().FirstOrDefaultAsync(s => s.OrderId == orderId);
        
        if (onUsPayment is null)
        {
            throw new NotFoundException(nameof(OnUsPayment), orderId);
        }

        var merchantTransaction = await _merchantTransactionRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == onUsPayment.MerchantTransactionId);

        if (merchantTransaction is null)
        {
            throw new NotFoundException(nameof(MerchantTransaction), onUsPayment.Id);
        }

        var inquireEmoneyProvision = await _emoneyProvisionService.InquireProvisionAsync(
            new InquireProvisionRequest
            {
                ConversationId = merchantTransaction.ConversationId
            });
        
        return new PosPaymentDetailResponse
        {
            IsSuccess = true,
            OrderStatus = GetOrderStatusFromEmoneyResponse(inquireEmoneyProvision),
            ResponseCode = string.IsNullOrEmpty(inquireEmoneyProvision.ErrorCode) ? GenericSuccessCode : inquireEmoneyProvision.ErrorCode,
            ResponseMessage = inquireEmoneyProvision.ErrorDescription
        };
    }

    private static OrderStatus GetOrderStatusFromEmoneyResponse(InquireProvisionResponse emoneyResponse)
    {
        if (!string.IsNullOrEmpty(emoneyResponse.ErrorCode))
        {
            return emoneyResponse.ErrorCode == EmoneyInquireNotFoundErrorCode ? OrderStatus.OrderNotFound : OrderStatus.Unknown;
        }

        return emoneyResponse.ProvisionStatus switch
        {
            ProvisionStatus.Returned or ProvisionStatus.PartiallyReturned => OrderStatus.Refunded,
            ProvisionStatus.Failed => OrderStatus.Rejected,
            ProvisionStatus.Chargeback => OrderStatus.Unknown,
            _ => OrderStatus.WaitingEndOfDay
        };
    }
    
    private async Task<ValidationResponse> PreValidateOnUsProvisionAsync(OnUsPayment onUsPayment, Merchant merchant, bool parentMerchantFinancialTransaction, VerifyOnUsPaymentCommand request)
    {
        if (onUsPayment is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidReferenceNumber, request.LanguageCode);
        }

        if ( !await 
                _merchantTransactionRepository
                    .GetAll()
                    .AnyAsync(s => s.Id == onUsPayment.MerchantTransactionId)
           )
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.MerchantTransactionNotFound, request.LanguageCode);
        }

        if (merchant is null || onUsPayment.MerchantId != merchant.Id)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode);
        }

        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode);
        }

        if (!merchant.IntegrationMode.ToString().Contains(IntegrationMode.OnUs.ToString()))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.IntegrationModeNotAllowed, request.LanguageCode);
        }

        if (onUsPayment.Status != ChannelStatus.Active || onUsPayment.PaymentStatus != ChannelPaymentStatus.Pending)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidOnUsStatus, request.LanguageCode);
        }

        return DateTime.Now.CompareTo(onUsPayment.ExpiryDate) >= 0 ? 
            await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.OnUsExpired, request.LanguageCode) : 
            new ValidationResponse { IsValid = true };
    }
    
    private async Task<ProvisionResponse> RetryDbUpdateAsync(MerchantTransaction merchantTransaction,
        EmoneyProvisionResponse emoneyProvisionResponse,
        BankTransaction bankTransaction,
        OnUsPayment onUsPayment,
        VerifyOnUsPaymentCommand request)
    {
        try
        {
            return emoneyProvisionResponse.IsSucceed
                ? await MarkAsCompletedAsync(merchantTransaction, bankTransaction, emoneyProvisionResponse, onUsPayment, request)
                : await MarkAsFailedAsync(merchantTransaction, bankTransaction, emoneyProvisionResponse, onUsPayment, request);
        }
        catch (Exception exception)
        {
            if (emoneyProvisionResponse.IsSucceed)
            {
                _logger.LogError($"MarkAsCompleteError: MerchantTransactionId : {merchantTransaction.Id}" +
                                 $" Error : {exception}");

                await _basePaymentService.InsertTimeoutTransactionAsync(merchantTransaction, bankTransaction, request.ClientIpAddress);
                
                onUsPayment.Status = ChannelStatus.Cancelled;
                onUsPayment.PaymentStatus = ChannelPaymentStatus.Failed;
                onUsPayment.Name = request.Name;
                onUsPayment.Surname = request.Surname;
                onUsPayment.Email = request.Email;
                onUsPayment.PhoneCode = request.PhoneCode;
                onUsPayment.PhoneNumber = request.PhoneNumber;
                onUsPayment.WalletNumber = request.WalletNumber;
                await _onUsPaymentRepository.UpdateAsync(onUsPayment);
            }
            else
            {
                _logger.LogError($"MarkAsFailedError: MerchantTransactionId : {merchantTransaction.Id} " +
                                 $"Error : {exception}");
            }

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                OrderId = string.Empty,
                ConversationId = request.ConversationId
            };
        }
    }
    
    private async Task<ProvisionResponse> MarkAsCompletedAsync(
        MerchantTransaction merchantTransaction,
        BankTransaction bankTransaction,
        EmoneyProvisionResponse emoneyProvisionResponse,
        OnUsPayment onUsPayment,
        VerifyOnUsPaymentCommand request)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            merchantTransaction.TransactionEndDate = DateTime.Now;
            merchantTransaction.ResponseCode = GenericSuccessCode;
            merchantTransaction.ResponseDescription = GenericSuccessCode;
            merchantTransaction.TransactionStatus = TransactionStatus.Success;
            merchantTransaction.ProvisionNumber = emoneyProvisionResponse.ReferenceNumber;
            merchantTransaction.IpAddress = request.ClientIpAddress;
            merchantTransaction.MerchantCustomerName = $"{request.Name} {request.Surname}";
            merchantTransaction.MerchantCustomerPhoneCode = request.PhoneCode;
            merchantTransaction.MerchantCustomerPhoneNumber = request.PhoneNumber;
            
            bankTransaction.TransactionEndDate = DateTime.Now;
            bankTransaction.BankOrderId = emoneyProvisionResponse.TransactionId.ToString();
            bankTransaction.ApprovalCode = emoneyProvisionResponse.ReferenceNumber;
            bankTransaction.BankResponseCode = GenericSuccessCode;
            bankTransaction.BankResponseDescription = GenericSuccessCode;
            bankTransaction.TransactionStatus = TransactionStatus.Success;
            bankTransaction.BankTransactionDate = DateTime.Now;

            onUsPayment.Status = ChannelStatus.Completed;
            onUsPayment.PaymentStatus = ChannelPaymentStatus.Success;
            onUsPayment.EmoneyReferenceNumber = emoneyProvisionResponse.ReferenceNumber;
            onUsPayment.EmoneyTransactionId = emoneyProvisionResponse.TransactionId;
            onUsPayment.Name = request.Name;
            onUsPayment.Surname = request.Surname;
            onUsPayment.Email = request.Email;
            onUsPayment.WalletNumber = request.WalletNumber;

            _dbContext.OnUsPayment.Update(onUsPayment);
            _dbContext.MerchantTransaction.Update(merchantTransaction);
            await _dbContext.AddAsync(bankTransaction);
            await _dbContext.SaveChangesAsync();
                
            scope.Complete();
        });
       
        try
        {
            await _basePaymentService.PublishIncrementLimitAsync(merchantTransaction.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"PublishIncrementLimitException: {exception}");
        }

        return new ProvisionResponse
        {
            IsSucceed = true,
            ConversationId = merchantTransaction.ConversationId,
            OrderId = bankTransaction.OrderId,
            ProvisionNumber = emoneyProvisionResponse.ReferenceNumber,
            TransactionId = merchantTransaction.Id,
        };
    }
    
    private async Task<ProvisionResponse> MarkAsFailedAsync(
        MerchantTransaction merchantTransaction,
        BankTransaction bankTransaction,
        EmoneyProvisionResponse emoneyProvisionResponse,
        OnUsPayment onUsPayment,
        VerifyOnUsPaymentCommand request)
    {
        var merchantError =
            await _errorCodeService.GetMerchantResponseCodeByBankCodeAsync(bankTransaction.AcquireBankCode,
                emoneyProvisionResponse.ErrorCode,
                emoneyProvisionResponse.ErrorMessage,
                merchantTransaction.LanguageCode);

        if (merchantError.ProcessTimeoutManagement)
        {
            await _basePaymentService.InsertTimeoutTransactionAsync(merchantTransaction, bankTransaction, request.ClientIpAddress);

            return new ProvisionResponse
            {
                IsSucceed = false,
                ErrorCode = merchantError.ResponseCode,
                ErrorMessage = merchantError.DisplayMessage,
                OrderId = string.Empty,
                ConversationId = merchantTransaction.ConversationId,
                TransactionId = merchantTransaction.Id,
                ProvisionNumber = emoneyProvisionResponse.ReferenceNumber
            };
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            merchantTransaction.ResponseCode = merchantError.ResponseCode;
            merchantTransaction.ResponseDescription = merchantError.DisplayMessage;
            merchantTransaction.TransactionEndDate = DateTime.Now;
            merchantTransaction.TransactionStatus = TransactionStatus.Fail;

            bankTransaction.BankResponseCode = emoneyProvisionResponse.ErrorCode;
            bankTransaction.BankResponseDescription = emoneyProvisionResponse.ErrorMessage;
            bankTransaction.TransactionEndDate = DateTime.Now;
            bankTransaction.TransactionStatus = TransactionStatus.Fail;
            bankTransaction.BankTransactionDate = DateTime.Now;
            
            onUsPayment.Status = ChannelStatus.Cancelled;
            onUsPayment.PaymentStatus = ChannelPaymentStatus.Failed;
            onUsPayment.EmoneyReferenceNumber = emoneyProvisionResponse.ReferenceNumber;
            onUsPayment.EmoneyTransactionId = emoneyProvisionResponse.TransactionId;
            onUsPayment.Name = request.Name;
            onUsPayment.Surname = request.Surname;
            onUsPayment.Email = request.Email;
            onUsPayment.WalletNumber = request.WalletNumber;

            _dbContext.OnUsPayment.Update(onUsPayment);
            _dbContext.MerchantTransaction.Update(merchantTransaction);
            await _dbContext.AddAsync(bankTransaction);

            await _dbContext.SaveChangesAsync();

            transactionScope.Complete();
        });

        return new ProvisionResponse
        {
            IsSucceed = false,
            ErrorMessage = merchantError.DisplayMessage,
            ErrorCode = merchantError.ResponseCode,
            OrderId = string.Empty,
            ConversationId = merchantTransaction.ConversationId,
            TransactionId = merchantTransaction.Id
        };
    }

    public async Task<PosVoidResponse> ReverseOnUsPayment(MerchantTransaction referenceMerchantTransaction)
    {
        var cancelProvisionResponse = await _emoneyProvisionService.CancelProvisionAsync(new ProvisionCancelRequest
        {
            ConversationId = referenceMerchantTransaction.ConversationId
        });
        
        var response = new PosVoidResponse();

        if (cancelProvisionResponse.IsSucceed)
        {
            response.IsSuccess = true;
            response.ResponseCode = GenericSuccessCode;
            response.AuthCode = cancelProvisionResponse.ConversationId;
            response.TransId = cancelProvisionResponse.TransactionId.ToString();
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = cancelProvisionResponse.ErrorCode;
            response.ResponseMessage = cancelProvisionResponse.ErrorMessage;
            response.TrxDate = DateTime.Now;
        }
        return response;
    }

    public async Task<PosRefundResponse> ReturnOnUsPayment(ReturnCommand request,
        MerchantTransaction referenceMerchantTransaction,
        Currency currency)
    {
        var onUsPayment = await _onUsPaymentRepository.GetAll()
            .FirstOrDefaultAsync(s => s.MerchantTransactionId == referenceMerchantTransaction.Id);

        if (onUsPayment is null)
        {
            throw new NotFoundException(nameof(OnUsPayment));
        }
        
        var returnProvisionResponse =
            await _emoneyProvisionService.ReturnProvisionAsync(new ProvisionReturnRequest
            {
                WalletNumber = onUsPayment.WalletNumber,
                Amount = request.Amount,
                CurrencyCode = currency.Code,
                ProvisionSource = ProvisionSource.Onus,
                ConversationId = request.ConversationId,
                ClientIpAddress = request.ClientIpAddress,
                ProvisionReferenceNumber = referenceMerchantTransaction.ProvisionNumber
            });
        
        var response = new PosRefundResponse();
        if (returnProvisionResponse.IsSucceed)
        {
            response.IsSuccess = true;
            response.ResponseCode = GenericSuccessCode;
            response.AuthCode = returnProvisionResponse.ReferenceNumber;
            response.TransId = returnProvisionResponse.TransactionId.ToString();
        }
        else
        {
            response.IsSuccess = false;
            response.ResponseCode = returnProvisionResponse.ErrorCode;
            response.ResponseMessage = returnProvisionResponse.ErrorMessage;
            response.TrxDate = DateTime.Now;
        }
        return response;
    }
}