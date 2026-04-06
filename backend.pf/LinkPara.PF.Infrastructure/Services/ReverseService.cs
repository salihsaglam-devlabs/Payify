using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Payments.Commands.Reverse;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.PF.Infrastructure.Services.VposServices;
using LinkPara.SharedModels.BusModels.Commands.PF;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Transactions;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using TransactionStatus = LinkPara.PF.Domain.Enums.TransactionStatus;

namespace LinkPara.PF.Infrastructure.Services;

public class ReverseService : IReverseService
{
    private const string GenericErrorCode = "99";

    private readonly ILogger<ReverseService> _logger;
    private readonly IMerchantService _merchantService;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
    private readonly IGenericRepository<BankTransaction> _bankTransactionRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IGenericRepository<MerchantApiValidationLog> _apiRequestValidationLogRepository;
    private readonly IGenericRepository<TimeoutTransaction> _timeoutTransactionRepository;
    private readonly IOrderNumberGeneratorService _orderNumberGeneratorService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly VposServiceFactory _vposServiceFactory;
    private readonly IContextProvider _contextProvider;
    private readonly IBus _bus;
    private readonly IFraudService _fraudService;
    private readonly IOnUsPaymentService _onUsPaymentService;
    private readonly IGenericRepository<SubMerchant> _subMerchantRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly ICurrencyService _currencyService;
    private readonly IGenericRepository<MerchantInstallmentTransaction> _merchantInstallmentTransactionRepository;
    public ReverseService(
        ILogger<ReverseService> logger,
        IMerchantService merchantService,
        IGenericRepository<Vpos> vposRepository,
        IGenericRepository<BankTransaction> bankTransactionRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IServiceScopeFactory scopeFactory,
        IResponseCodeService errorCodeService,
        IGenericRepository<MerchantApiValidationLog> apiRequestValidationLogRepository,
        IGenericRepository<TimeoutTransaction> timeoutTransactionRepository,
        IOrderNumberGeneratorService orderNumberGeneratorService,
        IApplicationUserService applicationUserService,
        VposServiceFactory vposServiceFactory,
        IContextProvider contextProvider,
        IBus bus,
        IFraudService fraudService,
        IOnUsPaymentService onUsPaymentService,
        ICurrencyService currencyService,
        IGenericRepository<SubMerchant> subMerchantRepository,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<MerchantVpos> merchantVposRepository,
        IGenericRepository<MerchantInstallmentTransaction> merchantInstallmentTransactionRepository)
    {
        _logger = logger;
        _vposRepository = vposRepository;
        _merchantService = merchantService;
        _bankTransactionRepository = bankTransactionRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
        _scopeFactory = scopeFactory;
        _errorCodeService = errorCodeService;
        _apiRequestValidationLogRepository = apiRequestValidationLogRepository;
        _timeoutTransactionRepository = timeoutTransactionRepository;
        _orderNumberGeneratorService = orderNumberGeneratorService;
        _applicationUserService = applicationUserService;
        _vposServiceFactory = vposServiceFactory;
        _contextProvider = contextProvider;
        _bus = bus;
        _fraudService = fraudService;
        _onUsPaymentService = onUsPaymentService;
        _subMerchantRepository = subMerchantRepository;
        _currencyService = currencyService;
        _merchantRepository = merchantRepository;
        _merchantVposRepository = merchantVposRepository;
        _merchantInstallmentTransactionRepository = merchantInstallmentTransactionRepository;
    }

    public async Task<ReverseResponse> ReverseAsync(ReverseCommand request)
    {
        try
        {
            var merchant = await _merchantService.GetByIdAsync(request.MerchantId);

            if (merchant is null)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode, request.ConversationId);
            }

            var parentMerchantFinancialTransaction = true;
            if (merchant.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
            {
                var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
                if (parentMerchant is not null)
                {
                    parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
                }
            }

            if (!merchant.PaymentReverseAllowed)
            {
                return await GetValidationResponseAsync(ApiErrorCode.NoReversePaymentAllowed, request.LanguageCode, request.ConversationId);
            }

            if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode, request.ConversationId);
            }

            var referenceBankTransaction = await _bankTransactionRepository.GetAll()
                  .FirstOrDefaultAsync(s =>
                       s.RecordStatus == RecordStatus.Active &&
                       s.OrderId == request.OrderId);

            if (referenceBankTransaction is null)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidReferenceNumber, request.LanguageCode, request.ConversationId);
            }

            var referenceMerchantTransaction = await _merchantTransactionRepository.GetAll()
                    .FirstOrDefaultAsync(s =>
                         s.RecordStatus == RecordStatus.Active &&
                         s.PfTransactionSource == PfTransactionSource.VirtualPos &&
                         s.TransactionStatus == TransactionStatus.Success &&
                         s.Id == referenceBankTransaction.MerchantTransactionId &&
                         s.MerchantId == merchant.Id &&
                         !s.IsReverse);

            if (referenceMerchantTransaction is null)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidReferenceNumber, request.LanguageCode, request.ConversationId);
            }

            if (referenceMerchantTransaction.IsChargeback || referenceMerchantTransaction.IsSuspecious)
            {
                return await GetValidationResponseAsync(ApiErrorCode.TransactionNotReversible, request.LanguageCode, request.ConversationId);
            }

            if (referenceMerchantTransaction.BatchStatus == BatchStatus.Completed)
            {
                return await GetValidationResponseAsync(ApiErrorCode.TransactionNotReversible, request.LanguageCode, request.ConversationId);
            }

            if (referenceMerchantTransaction.SubMerchantId.HasValue && referenceMerchantTransaction.SubMerchantId != Guid.Empty)
            {
                var subMerchant = await _subMerchantRepository.GetByIdAsync(referenceMerchantTransaction.SubMerchantId);

                if (subMerchant is null)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.InvalidSubMerchant, request.LanguageCode, request.ConversationId);
                }

                if (!subMerchant.PaymentReturnAllowed)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.SubMerchantNoReturnPaymentAllowed, request.LanguageCode, request.ConversationId);
                }

                if (subMerchant.RecordStatus != RecordStatus.Active)
                {
                    return await GetValidationResponseAsync(ApiErrorCode.InvalidSubMerchantStatus, request.LanguageCode, request.ConversationId);
                }
            }

            var vpos = await _vposRepository.GetAll()
                      .Include(s => s.AcquireBank)
                      .Include(s => s.VposBankApiInfos)
                      .ThenInclude(s => s.Key)
                      .FirstOrDefaultAsync(s => s.Id == referenceBankTransaction.VposId);

            if (vpos is null)
            {
                return await GetValidationResponseAsync(ApiErrorCode.InvalidReferenceNumber, request.LanguageCode, request.ConversationId);
            }

            if (!await _fraudService.CheckFraudAsync(new FraudTransactionDetail
            {
                Amount = referenceBankTransaction.Amount,
                BeneficiaryNumber = merchant.Number,
                Beneficiary = merchant.Name,
                BeneficiaryBankID = referenceBankTransaction.AcquireBankCode.ToString(),
                OriginatorNumber = referenceBankTransaction.CardNumber,
                Originator = referenceMerchantTransaction.CardHolderName,
                OriginatorBankID = referenceBankTransaction.IssuerBankCode.ToString() ?? string.Empty,
                FraudSource = FraudSource.Pos,
                Direction = Direction.Inbound,
                AmountCurrencyCode = referenceBankTransaction.Currency,
                BeneficiaryAccountCurrencyCode = referenceBankTransaction.Currency,
                OriginatorAccountCurrencyCode = referenceBankTransaction.Currency,
                Channel = _contextProvider.CurrentContext.Channel,
                TransactionType = TransactionType.Reverse.ToString(),
                MccCode = Convert.ToInt32(merchant.MccCode)
            }, "PfReverse", request.ClientIpAddress))
            {
                return await GetValidationResponseAsync(ApiErrorCode.PotentialFraud, request.LanguageCode, request.ConversationId);
            }

            return await ReverseAsync(request, referenceMerchantTransaction, referenceBankTransaction, vpos, merchant);

        }
        catch (PreValidationException exception)
        {
            var code = exception.Code;
            var message = exception.Message;

            _logger.LogError($"Reverse PreValidation failed with code : {code}, " +
                                 $"Message: {code}");

            await InsertValidationLogAsync(request, new ValidationResponse
            {
                Code = code,
                Message = message,
                IsValid = false
            });

            return new ReverseResponse
            {
                IsSucceed = false,
                ErrorCode = code,
                ConversationId = request.ConversationId,
                ErrorMessage = message
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Reverse PreValidation failed with code : {GenericErrorCode}, " +
                                $"Message: {exception.Message}");

            return new ReverseResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId
            };
        }
    }
    public async Task<ReverseResponse> ReverseAsync(ReverseCommand request, MerchantTransaction referenceMerchantTransaction,
        BankTransaction referenceBankTransaction, Vpos vpos, MerchantDto merchant)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? userId : _applicationUserService.ApplicationUserId.ToString();

        var bankTransaction = PopulateInitialBankTransaction(parseUserId);

        PosVoidResponse posResponse = null;

        try
        {
            var originalAuthProcess = await _merchantTransactionRepository.GetAll()
                .Where(s => s.ConversationId == referenceMerchantTransaction.ConversationId)
                .OrderBy(s => s.TransactionStartDate)
                .FirstOrDefaultAsync();

            var orgOrderId = referenceBankTransaction.OrderId;

            var currency = await _currencyService.GetByNumberAsync(referenceMerchantTransaction.Currency);

            bankTransaction.OrderId = await _orderNumberGeneratorService.GenerateForBankTransactionAsync(referenceBankTransaction.AcquireBankCode, merchant.Number);
            bankTransaction.Currency = referenceBankTransaction.Currency;
            bankTransaction.CardNumber = referenceBankTransaction.CardNumber;
            bankTransaction.VposId = referenceBankTransaction.VposId;
            bankTransaction.MerchantCode = referenceBankTransaction.MerchantCode;
            bankTransaction.SubMerchantCode = referenceBankTransaction.SubMerchantCode;
            bankTransaction.IssuerBankCode = referenceBankTransaction.IssuerBankCode;
            bankTransaction.AcquireBankCode = referenceBankTransaction.AcquireBankCode;
            bankTransaction.Amount = referenceBankTransaction.Amount;

            var merchantVpos = await GetSubMerchantCode(referenceMerchantTransaction.VposId, referenceMerchantTransaction.MerchantId);

            if (referenceMerchantTransaction.IsOnUsPayment)
            {
                posResponse = await _onUsPaymentService.ReverseOnUsPayment(referenceMerchantTransaction);
            }
            else
            {
                var bankService = _vposServiceFactory.GetVposServices(vpos, merchant.Id, referenceMerchantTransaction.IsInsurancePayment);

                posResponse = await bankService.Void(new PosVoidRequest
                {
                    SubMerchantCode = referenceBankTransaction.SubMerchantCode,
                    Currency = referenceMerchantTransaction.Currency,
                    OrderNumber = bankTransaction.OrderId,
                    OrgOrderNumber = orgOrderId,
                    LanguageCode = request.LanguageCode,
                    Amount = referenceBankTransaction.Amount,
                    ClientIp = request.ClientIpAddress,
                    SubMerchantId = merchant.Number,
                    SubMerchantName = merchant.Name,
                    SubMerchantCity = merchant.Customer.CityName,
                    SubMerchantTaxNumber = merchant.Customer.TaxNumber,
                    SubMerchantCountry = merchant.Customer.Country.ToString(),
                    SubMerchantMcc = merchant.MccCode,
                    SubMerchantUrl = merchant.WebSiteUrl,
                    SubMerchantGlobalMerchantId = merchant.GlobalMerchantId,
                    SubMerchantPostalCode = merchant.Customer.PostalCode,
                    BankOrderId = referenceBankTransaction.BankOrderId,
                    ProvisionNumber = referenceBankTransaction.ApprovalCode,
                    RRN = referenceBankTransaction.RrnNumber,
                    Stan = referenceBankTransaction.Stan,
                    TransactionType = referenceMerchantTransaction.TransactionType == TransactionType.PreAuth && referenceMerchantTransaction.TransactionStatus != TransactionStatus.Closed ? TransactionType.PreAuth : TransactionType.Auth,
                    ReverseType = referenceMerchantTransaction.TransactionType,
                    OrderDate = referenceMerchantTransaction.TransactionDate,
                    OrgAuthProcessOrderNo = originalAuthProcess.OrderId,
                    CurrencyCode = currency.Code,
                    IsTopUpPayment = request.IsTopUpPayment,
                    ServiceProviderPspMerchantId = merchantVpos.ServiceProviderPspMerchantId
                });
            }

            return posResponse.IsSuccess
                ? await MarkAsCompletedAsync(referenceMerchantTransaction, referenceBankTransaction, bankTransaction, posResponse, request.ConversationId)
                : await MarkAsFailedAsync(bankTransaction, posResponse, request.ConversationId, request.LanguageCode);

        }
        catch (TaskCanceledException exception)
               when (exception.InnerException is TimeoutException)
        {
            await InsertTimeoutTransactionAsync(referenceBankTransaction, referenceMerchantTransaction, bankTransaction, request.ClientIpAddress);

            return new ReverseResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId
            };
        }
        catch (Exception exception)
        {
            if (posResponse is not null)
            {
                return await RetryDbUpdateAsync(referenceMerchantTransaction, referenceBankTransaction,
                    bankTransaction, posResponse, request.ConversationId, request.LanguageCode, request.ClientIpAddress);
            }

            _logger.LogError(exception,
                "Reverse failed. ErrorCode: {GenericErrorCode}",
                GenericErrorCode);

            return new ReverseResponse
            {
                IsSucceed = false,
                ErrorCode = (exception is ApiException apiException)
                    ? apiException.Code
                    : GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId
            };
        }
    }
    private async Task<MerchantVpos> GetSubMerchantCode(Guid vposId, Guid merchantId)
    {
        var subMerchant = await _merchantVposRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.RecordStatus == RecordStatus.Active &&
                                      s.TerminalStatus == TerminalStatus.Active &&
                                      s.VposId == vposId &&
                                      s.MerchantId == merchantId);

        return subMerchant ?? new MerchantVpos();
    }
    private async Task<ReverseResponse> MarkAsCompletedAsync(MerchantTransaction referenceMerchantTransaction,
        BankTransaction referenceBankTransaction,
        BankTransaction newBankTransaction,
        PosVoidResponse posResponse,
        string conversationId)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? userId : _applicationUserService.ApplicationUserId.ToString();

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            if (referenceMerchantTransaction.TransactionType == TransactionType.Return)
            {
                var mainTransaction = await _merchantTransactionRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.Id == Guid.Parse(referenceMerchantTransaction.ReturnedTransactionId));

                var successfullyReturnedTransactions = _merchantTransactionRepository.GetAll()
                    .Where(s => s.TransactionType == TransactionType.Return &&
                            s.TransactionStatus == TransactionStatus.Success &&
                            s.ReturnedTransactionId == mainTransaction.Id.ToString());

                var countOfReturnedTransactions = successfullyReturnedTransactions.Count();
                var sumOfReturnedTransactionAmounts = successfullyReturnedTransactions.Sum(s => s.Amount);

                var lastReturnTransaction = await successfullyReturnedTransactions
                .Where(s => s.Id != referenceMerchantTransaction.Id)
                .OrderByDescending(s => s.CreateDate)
                .FirstOrDefaultAsync();

                var sumOfReversedReturnTransactions = _merchantTransactionRepository.GetAll()
                    .Where(s =>
                            s.TransactionType == TransactionType.Return &&
                            s.TransactionStatus == TransactionStatus.Reversed &&
                            s.ReturnedTransactionId == mainTransaction.Id.ToString())
                    .Sum(s => s.Amount);

                var allAmountReversed = mainTransaction.Amount == (referenceMerchantTransaction.Amount + sumOfReversedReturnTransactions);

                if (allAmountReversed || countOfReturnedTransactions <= 1)
                {
                    mainTransaction.TransactionStatus = TransactionStatus.Success;
                    mainTransaction.ReturnAmount = 0;
                    mainTransaction.IsReturn = false;
                }
                else
                {
                    mainTransaction.ReturnAmount = sumOfReturnedTransactionAmounts - referenceMerchantTransaction.Amount;
                    mainTransaction.TransactionStatus = TransactionStatus.PartiallyReturned;
                    mainTransaction.IsReturn = true;
                }

                mainTransaction.ReturnDate = (allAmountReversed || lastReturnTransaction is null)
                    ? DateTime.MinValue
                    : lastReturnTransaction.CreateDate;

                mainTransaction.BankCommissionAmount = allAmountReversed
                    ? (mainTransaction.BankCommissionRate / 100m) * mainTransaction.Amount
                    : (mainTransaction.BankCommissionRate / 100m) * (mainTransaction.Amount - mainTransaction.ReturnAmount);

                dbContext.Update(mainTransaction);
            }

            if (referenceMerchantTransaction.TransactionType == TransactionType.PostAuth)
            {
                var preAuthMerchantTransaction = await _merchantTransactionRepository.GetAll()
                    .FirstOrDefaultAsync(s =>
                  s.RecordStatus == RecordStatus.Active &&
                  (s.TransactionStatus == TransactionStatus.Closed ||
                  s.TransactionStatus == TransactionStatus.PartiallyClosed) &&
                  s.IsPreClose == true &&
                  s.MerchantId == referenceMerchantTransaction.MerchantId &&
                  s.PreCloseTransactionId == referenceMerchantTransaction.Id.ToString());

                preAuthMerchantTransaction.TransactionStatus = TransactionStatus.Success;
                preAuthMerchantTransaction.IsPreClose = false;
                preAuthMerchantTransaction.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                dbContext.Update(preAuthMerchantTransaction);
            }

            referenceMerchantTransaction.IsReverse = true;
            referenceMerchantTransaction.ReverseDate = DateTime.Now;
            referenceMerchantTransaction.TransactionStatus = TransactionStatus.Reversed;
            referenceMerchantTransaction.LastModifiedBy = parseUserId;

            dbContext.Update(referenceMerchantTransaction);

            referenceBankTransaction.IsReverse = true;
            referenceBankTransaction.ReverseDate = DateTime.Now;
            referenceBankTransaction.TransactionStatus = TransactionStatus.Reversed;
            referenceBankTransaction.LastModifiedBy = parseUserId;

            dbContext.Update(referenceBankTransaction);

            newBankTransaction.TransactionEndDate = DateTime.Now;
            newBankTransaction.BankOrderId = posResponse.OrderNumber;
            newBankTransaction.ApprovalCode = posResponse.AuthCode;
            newBankTransaction.RrnNumber = posResponse.RrnNumber;
            newBankTransaction.BankResponseCode = posResponse.ResponseCode;
            newBankTransaction.BankResponseDescription = posResponse.ResponseMessage;
            newBankTransaction.TransactionStatus = TransactionStatus.Success;
            newBankTransaction.BankTransactionDate = posResponse.TrxDate;

            await dbContext.AddAsync(newBankTransaction);

            if (referenceMerchantTransaction.IsPerInstallment == true)
            {
                var installmentTransactions = await _merchantInstallmentTransactionRepository.GetAll().Where(b => b.MerchantTransactionId == referenceMerchantTransaction.Id).ToListAsync();

                if (installmentTransactions.Any())
                {
                    if (referenceMerchantTransaction.TransactionType == TransactionType.PostAuth)
                    {
                        var installmentIds = installmentTransactions
                            .Select(s => s.Id.ToString())
                            .ToList();

                        var preAuthInstallments = await _merchantInstallmentTransactionRepository.GetAll()
                            .Where(s =>
                                s.RecordStatus == RecordStatus.Active &&
                                (s.TransactionStatus == TransactionStatus.Closed ||
                                 s.TransactionStatus == TransactionStatus.PartiallyClosed) &&
                                s.IsPreClose == true &&
                                s.MerchantId == referenceMerchantTransaction.MerchantId &&
                                installmentIds.Contains(s.PreCloseTransactionId))
                            .ToListAsync();

                        foreach (var preAuth in preAuthInstallments)
                        {
                            preAuth.TransactionStatus = TransactionStatus.Success;
                            preAuth.IsPreClose = false;
                            preAuth.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                        }

                        dbContext.UpdateRange(preAuthInstallments);
                    }

                    foreach (var item in installmentTransactions)
                    {
                        item.IsReverse = true;
                        item.ReverseDate = DateTime.Now;
                        item.TransactionStatus = TransactionStatus.Reversed;
                        item.LastModifiedBy = parseUserId;
                    }

                    dbContext.UpdateRange(installmentTransactions);
                }
            }

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();
        });

        try
        {
            await PublishDecrementLimitAsync(referenceMerchantTransaction.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"PublishDecrementLimitException: {exception}");
        }

        return new ReverseResponse
        {
            IsSucceed = true,
            ConversationId = conversationId,
            ProvisionNumber = posResponse.AuthCode
        };
    }
    private async Task PublishDecrementLimitAsync(Guid merchantTransactionId)
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.DecrementLimits"));
        await endpoint.Send(new IncrementLimits
        {
            MerchantTransactionId = merchantTransactionId
        }, tokenSource.Token);
    }
    private async Task<ReverseResponse> MarkAsFailedAsync(
        BankTransaction newBankTransaction,
        PosVoidResponse posResponse,
        string conversationId,
        string languageCode)
    {
        var merchantError =
              await _errorCodeService.GetMerchantResponseCodeByBankCodeAsync(newBankTransaction.AcquireBankCode,
                  posResponse.ResponseCode, posResponse.ResponseMessage, languageCode);

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PfDbContext>();
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            newBankTransaction.BankResponseCode = posResponse.ResponseCode;
            newBankTransaction.BankResponseDescription = posResponse.ResponseMessage;
            newBankTransaction.TransactionEndDate = DateTime.Now;
            newBankTransaction.TransactionStatus = TransactionStatus.Fail;
            newBankTransaction.BankTransactionDate = posResponse.TrxDate;

            await dbContext.AddAsync(newBankTransaction);

            await dbContext.SaveChangesAsync();

            transactionScope.Complete();
        });
        return new ReverseResponse
        {
            IsSucceed = false,
            ErrorCode = merchantError.ResponseCode,
            ErrorMessage = merchantError.DisplayMessage,
            ConversationId = conversationId
        };
    }

    private static BankTransaction PopulateInitialBankTransaction(string parseUserId)
    {
        var bankTransaction = new BankTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedBy = parseUserId,
            TransactionStartDate = DateTime.Now,
            TransactionStatus = TransactionStatus.Pending,
            TransactionType = TransactionType.Reverse,
            Amount = 0,
            PointAmount = 0,
            InstallmentCount = 0,
            Is3ds = false,
            IsReverse = false,
            MerchantTransactionId = Guid.Empty,
            EndOfDayStatus = EndOfDayStatus.Pending
        };
        return bankTransaction;
    }

    private async Task<ReverseResponse> GetValidationResponseAsync(string errorCode, string languageCode, string conversationId)
    {
        var apiResponse = await _errorCodeService.GetApiResponseCode(errorCode, languageCode);

        return new ReverseResponse
        {
            IsSucceed = false,
            ErrorCode = apiResponse.ResponseCode,
            ConversationId = conversationId,
            ErrorMessage = apiResponse.DisplayMessage
        };
    }

    private async Task InsertTimeoutTransactionAsync(BankTransaction referenceBankTransaction,
        MerchantTransaction referenceMerchantTransaction,
        BankTransaction bankTransaction,
        string clientIpAddress)
    {
        await _timeoutTransactionRepository.AddAsync(new TimeoutTransaction
        {
            MerchantId = referenceMerchantTransaction.MerchantId,
            ConversationId = referenceMerchantTransaction.ConversationId,
            TransactionType = referenceMerchantTransaction.TransactionType == TransactionType.PreAuth && referenceMerchantTransaction.TransactionStatus != TransactionStatus.Closed ? TransactionType.PreAuth : TransactionType.Auth,
            TimeoutTransactionStatus = TimeoutTransactionStatus.Pending,
            TransactionDate = DateTime.Now,
            VposId = referenceBankTransaction.VposId,
            MerchantTransactionId = Guid.Empty,
            BankTransactionId = referenceBankTransaction.Id,
            AcquireBankCode = referenceBankTransaction.AcquireBankCode,
            OriginalOrderId = referenceBankTransaction.OrderId,
            OrderId = bankTransaction.OrderId,
            SubMerchantCode = referenceBankTransaction.SubMerchantCode,
            LanguageCode = referenceMerchantTransaction.LanguageCode,
            Currency = referenceBankTransaction.Currency,
            Amount = referenceBankTransaction.Amount,
            CardNumber = referenceBankTransaction.CardNumber,
            CreateDate = DateTime.Now,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            RecordStatus = RecordStatus.Active,
            Description = "Transaction Timeout",
            ClientIpAddress = clientIpAddress
        });
    }

    private async Task InsertValidationLogAsync(ReverseCommand request, ValidationResponse validationResponse)
    {
        await _apiRequestValidationLogRepository.AddAsync(new MerchantApiValidationLog
        {
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            Amount = 0,
            Currency = string.Empty,
            MerchantId = request.MerchantId,
            CardToken = string.Empty,
            ConversationId = request.ConversationId,
            InstallmentCount = 0,
            LanguageCode = request.LanguageCode,
            PointAmount = 0,
            TransactionType = TransactionType.Reverse,
            ClientIpAddress = request.ClientIpAddress,
            OriginalReferenceNumber = request.OrderId,
            ThreeDSessionId = string.Empty,
            ErrorCode = validationResponse.Code,
            ErrorMessage = validationResponse.Message
        });
    }

    private async Task<ReverseResponse> RetryDbUpdateAsync(MerchantTransaction referenceMerchantTransaction,
        BankTransaction referenceBankTransaction, BankTransaction bankTransaction,
        PosVoidResponse posResponse,
        string conversationId,
        string languageCode,
        string clientIpAddress)
    {
        try
        {
            return posResponse.IsSuccess
                ? await MarkAsCompletedAsync(referenceMerchantTransaction, referenceBankTransaction, bankTransaction, posResponse, conversationId)
                : await MarkAsFailedAsync(bankTransaction, posResponse, conversationId, languageCode);
        }
        catch (Exception exception)
        {
            if (posResponse.IsSuccess)
            {
                _logger.LogError($"Reverse MarkAsCompleteError: ReferenceMerchantTransactionId : {referenceMerchantTransaction.Id}" +
                                 $" Error : {exception}");

                await InsertTimeoutTransactionAsync(referenceBankTransaction, referenceMerchantTransaction, bankTransaction, clientIpAddress);
            }
            else
            {
                _logger.LogError($"Reverse MarkAsFailedError: ReferenceMerchantTransactionId : {referenceMerchantTransaction.Id} " +
                                 $"Error : {exception}");
            }

            return new ReverseResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = conversationId
            };
        }
    }
}
