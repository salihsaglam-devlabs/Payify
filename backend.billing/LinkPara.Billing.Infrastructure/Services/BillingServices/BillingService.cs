using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;
using LinkPara.Billing.Application.Features.InstitutionApis.Queries.GetBillInquiry;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.SharedModels.Exceptions;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using Microsoft.Extensions.Logging;
using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.BusinessParameter;
using Newtonsoft.Json;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Emoney;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Persistence;
using LinkPara.Billing.Application.Features.Reconciliations.Commands.ReconciliationJob;
using LinkPara.HttpProviders.Vault;
using Microsoft.Extensions.Configuration;
using MassTransit;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using System;
using System.Globalization;
using Microsoft.Extensions.Localization;

namespace LinkPara.Billing.Infrastructure.Services.BillingServices;

public class BillingService : IBillingService
{
    private readonly IInstitutionService _institutionService;
    private readonly IBillingVendorServiceFactory _billingVendorServiceFactory;
    private readonly IContextProvider _contextProvider;
    private readonly ITransactionService _transactionService;
    private readonly IGenericRepository<Summary> _reconciliationRecordsRepository;
    private readonly IGenericRepository<InstitutionSummary> _reconciliationDetailRecordRepository;
    private readonly IGenericRepository<InstitutionDetail> _reconciliationInstitutionRecordRepository;
    private readonly IGenericRepository<TimeoutTransaction> _timeoutTransactionRepository;
    private readonly IGenericRepository<Vendor> _vendorRepository;
    private readonly ILogger<BillingService> _logger;
    private readonly IAccountingService _accountingService;
    private readonly IEmoneyService _eMoneyService;
    private readonly IAuditLogService _auditLogService;
    private readonly IFraudTransactionService _fraudTransactionService;
    private readonly IParameterService _parameterService;
    private readonly IAccountService _accountService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IVaultClient _vaultClient;
    private readonly IConfiguration _configuration;
    private readonly IBus _bus;
    private readonly IStringLocalizer _exceptionLocalizer;

    private const int reconciliationPeriod = 30;
    private Vendor _activeVendor;

    public BillingService(IInstitutionService institutionService,
        IBillingVendorServiceFactory billingVendorServiceFactory,
        IContextProvider contextProvider,
        IFraudTransactionService fraudTransactionService,
        IGenericRepository<Summary> reconciliationRecordsRepository,
        IAccountingService accountingService,
        IGenericRepository<InstitutionSummary> reconciliationDetailRecordRepository,
        ILogger<BillingService> logger,
        IGenericRepository<InstitutionDetail> reconciliationInstitutionRecordRepository,
        IEmoneyService eMoneyService,
        IGenericRepository<TimeoutTransaction> timeoutTransactionRepository,
        IAuditLogService auditLogService,
        ITransactionService transactionService,
        IParameterService parameterService,
        IAccountService accountService,
        IApplicationUserService applicationUserService,
        IVaultClient vaultClient,
        IConfiguration configuration,
        IBus bus,
        IGenericRepository<Vendor> vendorRepository,
        IStringLocalizerFactory stringLocalizerFactory)
    {
        _institutionService = institutionService;
        _billingVendorServiceFactory = billingVendorServiceFactory;
        _contextProvider = contextProvider;
        _transactionService = transactionService;
        _reconciliationRecordsRepository = reconciliationRecordsRepository;
        _accountingService = accountingService;
        _reconciliationDetailRecordRepository = reconciliationDetailRecordRepository;
        _logger = logger;
        _reconciliationInstitutionRecordRepository = reconciliationInstitutionRecordRepository;
        _eMoneyService = eMoneyService;
        _timeoutTransactionRepository = timeoutTransactionRepository;
        _auditLogService = auditLogService;
        _fraudTransactionService = fraudTransactionService;
        _parameterService = parameterService;
        _accountService = accountService;
        _applicationUserService = applicationUserService;
        _vaultClient = vaultClient;
        _configuration = configuration;
        _bus = bus;
        _vendorRepository = vendorRepository;

        _exceptionLocalizer = stringLocalizerFactory.Create("Exceptions", "LinkPara.Billing.API");
    }

    public async Task<BillingResponse<BillInquiryResponse>> InquireBillsAsync(InquireBillQuery request)
    {
        try
        {
            var vendorService = await GetVendorService(request.InstitutionId);

            await vendorService.ValidateRequestAsync(request.InstitutionId, request.SubscriberNumber1, request.SubscriberNumber2, request.SubscriberNumber3);

            return await vendorService.InquireBillsAsync(request);
        }
        catch (Exception exception)
        {
            if (exception is TimeoutException || exception.InnerException is TimeoutException)
            {
                throw new ExternalApiException("TimeoutWhenInquiringBill!");
            }

            throw;
        }
    }

    public async Task<BillingResponse<BillPaymentResponse>> PayInquiredBillAsync(PayInquiredBillCommand request)
    {
        var vendorService = await GetVendorService(request.InstitutionId);
        var userId = _contextProvider.CurrentContext.UserId;
        var provisionPreviewRequest = new ProvisionPreviewRequest
        {
            Amount = request.Bill.Amount + request.Bill.CommissionAmount + request.Bill.BsmvAmount,
            CurrencyCode = request.Bill.Currency,
            WalletNumber = request.WalletNumber,
            UserId = Guid.Parse(userId)
        };
        var provisionPreview = await _eMoneyService.PreviewProvisionAsync(provisionPreviewRequest);
        var billTransaction = PopulateTransaction(request);
        var paymentResponse = new BillingResponse<BillPaymentResponse>();

        if (!provisionPreview.IsSuccess)
        {
            _logger.LogError("ProvisionPreviewError: {ErrorMessage}", provisionPreview.ErrorMessage);

            billTransaction.TransactionStatus = TransactionStatus.ProvisionError;
            billTransaction.ErrorMessage = $"ProvisionError: {provisionPreview.ErrorMessage}";

            await _transactionService.AddAsync(billTransaction);

            if (provisionPreview.ErrorCode == "EMN002")
            {
                throw new InsufficientBalanceException();
            }

            throw new ProvisionPreviewException();
        }

        try
        {
            var account = await _accountService
                    .GetAccountDetailAsync(new GetAccountDetailRequest { UserId = Guid.Parse(userId) });

            var currencyCode = await GetCurrencyCode(request.Bill.Currency);

            var IsTransactionCheckEnabled =
           _vaultClient.GetSecretValue<bool>("/SharedSecrets", "ServiceState", "TransactionEnabled");

            if (IsTransactionCheckEnabled)
            {
                var requestModel = new FraudTransactionDetail
                {
                    Amount = request.Bill.Amount,
                    BeneficiaryNumber = request.Bill.Number,
                    Beneficiary = request.Bill.SubscriberName,
                    OriginatorNumber = request.WalletNumber,
                    Originator = request.PayeeFullName,
                    FraudSource = FraudSource.Wallet,
                    Direction = Direction.Outbound,
                    AmountCurrencyCode = Convert.ToInt32(currencyCode),
                    BeneficiaryAccountCurrencyCode = Convert.ToInt32(currencyCode),
                    OriginatorAccountCurrencyCode = Convert.ToInt32(currencyCode),
                    Channel = _contextProvider.CurrentContext.Channel
                };

                await CheckFraudAsync(new FraudCheckRequest
                {
                    CommandName = "Invoice",
                    ExecuteTransaction = requestModel,
                    UserId = userId,
                    Module = "Emoney",
                    AccountKycLevel = account.AccountKycLevel,
                    CommandJson = JsonConvert.SerializeObject(requestModel)
                });
            }

            paymentResponse = await vendorService.PayInquiredBillsAsync(request);

            if (paymentResponse.IsSuccess && paymentResponse.Response?.Invoice != null)
            {
                billTransaction.Description = paymentResponse.Response.Invoice.Description;
                billTransaction.ReferenceId = paymentResponse.Response.Invoice.ReferenceId;
                billTransaction.VoucherNumber = paymentResponse.Response.Invoice.VoucherNumber;

                var invoice = paymentResponse.Response.Invoice;

                if (invoice.IsSuccess)
                {
                    var institution = await _institutionService.GetByIdAsync(billTransaction.InstitutionId);
                    var provisionRequest = new ProvisionRequest
                    {
                        Amount = request.Bill.Amount,
                        ClientIpAddress = _contextProvider.CurrentContext.ClientIpAddress,
                        ConversationId = Guid.NewGuid().ToString(),
                        ProvisionSource = ProvisionSource.Billing,
                        UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
                        WalletNumber = request.WalletNumber,
                        Description = $"{request.InstitutionId}-{request.Bill.Number}-{request.WalletNumber}",
                        CurrencyCode = request.Bill.Currency,
                        Tag = $"{institution.Name} (Abone: {billTransaction.SubscriptionNumber1})",
                        BsmvAmount = request.Bill.BsmvAmount,
                        CommissionAmount = request.Bill.CommissionAmount
                    };
                    var provision = await _eMoneyService.CreateProvisionAsync(provisionRequest);
                    paymentResponse.Response.EmoneyTransactionId = provision.TransactionId;
                    if (provision.IsSucceed)
                    {
                        billTransaction.ProvisionReferenceId = provision.ConversationId;
                        billTransaction.TransactionStatus = TransactionStatus.Paid;

                        await _accountingService.PostAccountingPaymentAsync(billTransaction, provision.TransactionId);
                    }
                    else
                    {
                        _logger.LogCritical("ProvisionErrorDespiteSuccessfulBillPayment, ProvisionReference {ProvisionReference}, Error: {ErrorMessage}",
                     provisionRequest.ConversationId, provision.ErrorMessage);


                        billTransaction.ErrorMessage = provision.ErrorMessage;
                        billTransaction.ErrorCode = provision.ErrorCode;
                        billTransaction.TransactionStatus = TransactionStatus.Poison;
                    }
                }
                else
                {
                    var errorMessage = $"ErrorPayingBill, ErrorFromVendor: {invoice.Description}";

                    _logger.LogError("{errorMessage}, Invoice: {invoice}", errorMessage, invoice);

                    throw new ExternalApiException(errorMessage);
                }
            }
            else
            {
                billTransaction.ErrorMessage = paymentResponse.ErrorMessage;
                billTransaction.ErrorCode = paymentResponse.ErrorCode;
            }

            await _transactionService.AddAsync(billTransaction);
        }
        catch (Exception exception)
            when (exception.InnerException is TimeoutException
                 || exception is TimeoutException)
        {
            billTransaction.ErrorMessage = "TimeoutError";
            billTransaction.ErrorCode = ApiErrorCode.TimeoutError;
            billTransaction.TransactionStatus = TransactionStatus.Timeout;

            await _transactionService.AddAsync(billTransaction);

            var timeoutTransaction = PopulateTimeoutTransaction(billTransaction);

            await _timeoutTransactionRepository.AddAsync(timeoutTransaction);

            paymentResponse.IsSuccess = false;
            paymentResponse.ErrorCode = ApiErrorCode.TimeoutError;
            paymentResponse.ErrorMessage = "TimeoutError";
        }

        return paymentResponse;
    }

    private async Task CheckFraudAsync(FraudCheckRequest requestFraud)
    {
        var transaction = new TransactionResponse();
        try
        {
            transaction = await _fraudTransactionService.ExecuteTransaction(requestFraud);
        }
        catch (Exception exception)
        {
            _logger.LogCritical("Fraud Billing Error : {exception}", exception);
        }

        if (transaction.IsSuccess)
        {
            int riskLevel;
            try
            {
                var parameter = await _parameterService.GetParameterAsync("FraudParameters", "RiskLevel");
                riskLevel = Convert.ToInt32(parameter.ParameterValue);
            }
            catch (Exception exception)
            {
                _logger.LogError("GetParameterAsync Error : {exception} ", exception);
                riskLevel = (int)RiskLevel.Critical;
            }

            if ((int)transaction.RiskLevel >= riskLevel)
            {
                var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

                var exceptionMessage = _exceptionLocalizer.GetString("PotentialFraudException");

                throw new PotentialFraudException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
            }
        }
        else
        {
            _logger.LogError("Fraud Billing Error : {ErrorMessage}", transaction.ErrorMessage);
        }
    }
    private async Task<string> GetCurrencyCode(string currencyCode)
    {
        var parameterTemplateValue = await _parameterService
        .GetAllParameterTemplateValuesAsync("Currencies", currencyCode);

        return parameterTemplateValue?.Find(b => b.TemplateCode == "Number")?.TemplateValue;
    }
    public async Task<BillingResponse<BillCancelResponse>> CancelBillPaymentAsync(Guid transactionId, string cancellationReason)
    {
        var transaction = await _transactionService.GetByIdAsync(transactionId);
        var daysPassed = (DateTime.Now - transaction.PaymentDate).TotalDays;

        if (daysPassed > 1)
        {
            throw new BillCancelDayLimitException();
        }

        var billCancelRequest = PopulateBillCancelRequest(transaction, cancellationReason);
        var vendorService = await GetVendorService(billCancelRequest.InstitutionId);
        var billPaymentCancelResponse = await vendorService.CancelBillPaymentAsync(billCancelRequest);

        if (billPaymentCancelResponse.IsSuccess && billPaymentCancelResponse.Response.BillCancelInvoice != null && billPaymentCancelResponse.Response.BillCancelInvoice.IsSuccess)
        {
            transaction.Description = billPaymentCancelResponse.Response.BillCancelInvoice.Description;
            transaction.VoucherNumber = billPaymentCancelResponse.Response.BillCancelInvoice.VoucherNumber;
            transaction.ReferenceId = billPaymentCancelResponse.Response.BillCancelInvoice.ReferenceId;
            transaction.TransactionStatus = TransactionStatus.Cancelled;
            transaction.UpdateDate = DateTime.Now;

            var provisionResponse = await _eMoneyService.CancelProvisionAsync(transaction.ProvisionReferenceId);

            if (!provisionResponse.IsSucceed)
            {
                _logger.LogCritical("ProvisionCancelFailedAfterSuccessfulBillPayment. ProvisionReference: {ProvisionReferenceId}, Error: {ErrorMessage}",
                     transaction.ProvisionReferenceId, provisionResponse.ErrorMessage);


                transaction.TransactionStatus = TransactionStatus.Poison;
            }

            billPaymentCancelResponse.Response.EmoneyTransactionId = provisionResponse.TransactionId;

            await _accountingService.CancelAccountingPaymentAsync(transaction);

            await _transactionService.UpdateAsync(transaction);

        }

        return billPaymentCancelResponse;
    }

    public async Task<BillingResponse<BillStatusResponse>> InquireBillStatusAsync(Guid transactionId)
    {
        var transaction = await _transactionService.GetByIdAsync(transactionId);
        var vendorService = await GetVendorService(transaction.InstitutionId);
        var billStatusRequest = new BillStatusRequest
        {
            BillId = transaction.BillId,
            RequestId = transaction.ServiceRequestId,
            VendorId = transaction.VendorId,
            InstitutionId = transaction.InstitutionId
        };

        return await vendorService.InquireBillStatusAsync(billStatusRequest);
    }

    public async Task<BillingResponse<ReconciliationSummaryResponse>> GetReconciliationSummaryAsync(ReconciliationSummaryRequest request)
    {
        var reconcilationResponse = new BillingResponse<ReconciliationSummaryResponse>();
        var saveSummary = true;

        try
        {
            var existingSummary = await _reconciliationRecordsRepository
                .GetAll()
                .FirstOrDefaultAsync(r => r.ReconciliationDate.Date == request.ReconciliationDate.Date
                    && r.ReconciliationStatus == ReconciliationStatus.Success
                    && r.RecordStatus == RecordStatus.Active);

            if (existingSummary != null)
            {
                var errorMessage = $"ReconciliationAlreadyDoneForThisDate: {request.ReconciliationDate.Date}";

                _logger.LogError(errorMessage);

                throw new InvalidOperationException(errorMessage);
            }

            var transactionStatistics = await _transactionService.GetTransactionStatisticsAsync(request.VendorId, request.ReconciliationDate);
            var billingService = await _billingVendorServiceFactory.GetBillingServiceAsync(request.VendorId);
            var reconcilationRecord = new Summary
            {
                VendorId = request.VendorId,
                TotalCancelAmount = transactionStatistics.CancellationAmount,
                TotalCancelCount = transactionStatistics.CancellationCount,
                TotalPaymentAmount = transactionStatistics.PaymentAmount,
                TotalPaymentCount = transactionStatistics.PaymentCount,
                VendorTotalCancelAmount = 0,
                VendorTotalCancelCount = 0,
                VendorTotalPaymentAmount = 0,
                VendorTotalPaymentCount = 0,
                ReconciliationDate = request.ReconciliationDate,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString()
            };

            reconcilationResponse = await billingService.GetReconciliationSummaryAsync(transactionStatistics);

            var oldSummaries = await _reconciliationRecordsRepository
                    .GetAll()
                    .Where(w => w.ReconciliationDate.Date == request.ReconciliationDate.Date
                        && w.RecordStatus == RecordStatus.Active
                     )
                    .ToListAsync();

            oldSummaries.ForEach(s => s.RecordStatus = RecordStatus.Passive);

            await _reconciliationRecordsRepository.UpdateRangeAsync(oldSummaries);

            if (reconcilationResponse?.Response != null)
            {
                reconcilationRecord.VendorTotalCancelAmount = reconcilationResponse.Response.CancellationAmount;
                reconcilationRecord.VendorTotalCancelCount = reconcilationResponse.Response.CancellationCount;
                reconcilationRecord.VendorTotalPaymentAmount = reconcilationResponse.Response.PaymentAmount;
                reconcilationRecord.VendorTotalPaymentCount = reconcilationResponse.Response.PaymentCount;
                reconcilationRecord.ReconciliationDate = reconcilationResponse.Response.ReconciliationDate;
                reconcilationRecord.ReconciliationStatus = reconcilationResponse.Response.ReconciliationStatus;
                reconcilationRecord.Explanation = reconcilationResponse.ErrorMessage;
            }
            else
            {
                reconcilationRecord.VendorTotalCancelAmount = 0;
                reconcilationRecord.VendorTotalCancelCount = 0;
                reconcilationRecord.VendorTotalPaymentAmount = 0;
                reconcilationRecord.VendorTotalPaymentCount = 0;
                reconcilationRecord.ReconciliationDate = request.ReconciliationDate;
                reconcilationRecord.ReconciliationStatus = ReconciliationStatus.Fail;
                reconcilationRecord.Explanation = reconcilationResponse?.ErrorMessage;
            }

            await _reconciliationRecordsRepository.AddAsync(reconcilationRecord);

            saveSummary = false;

            await _auditLogService.AuditLogAsync(new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "AddSummary",
                SourceApplication = "Billing",
                Resource = "Summary",
                Details = new Dictionary<string, string>
            {
                      {"Id", reconcilationRecord.Id.ToString()},
                      {"VendorId", reconcilationRecord.VendorId.ToString() }
            }
            });
        }
        catch (Exception exception)
            when (exception.InnerException is TimeoutException
                || exception is TimeoutException
                || exception is ReconciliationException)
        {
            reconcilationResponse.IsSuccess = false;
            reconcilationResponse.ErrorMessage = exception.Message;
            reconcilationResponse.ErrorCode = ApiErrorCode.TimeoutError;

            if (saveSummary)
            {
                var reconciliationSummary = new Summary
                {
                    VendorId = request.VendorId,
                    TotalPaymentAmount = 0,
                    TotalPaymentCount = 0,
                    TotalCancelAmount = 0,
                    TotalCancelCount = 0,
                    VendorTotalPaymentAmount = 0,
                    VendorTotalPaymentCount = 0,
                    VendorTotalCancelAmount = 0,
                    VendorTotalCancelCount = 0,
                    ReconciliationDate = request.ReconciliationDate,
                    ReconciliationStatus = ReconciliationStatus.Fail,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    Explanation = reconcilationResponse.ErrorMessage
                };

                await _reconciliationRecordsRepository.AddAsync(reconciliationSummary);
            }

            if (exception is ReconciliationException)
            {
                reconcilationResponse.ErrorCode = ApiErrorCode.ReconciliationJobError;
            }
        }

        return reconcilationResponse;
    }

    public async Task<BillingResponse<ReconciliationDetailsResponse>> GetReconciliationDetailsAsync(ReconciliationDetailsRequest request)
    {
        var billingService = await _billingVendorServiceFactory.GetBillingServiceAsync(request.VendorId);
        var detailsResponse = await billingService.GetReconciliationDetailsAsync(request);

        if (detailsResponse.IsSuccess)
        {
            try
            {
                await SetOldInstitutionSummariesPassive(request);

                var institutionsStatistics = await _transactionService.GetTransactionStatisticsByInstitutionAsync(request.VendorId, request.InstitutionId, request.ReconciliationDate);

                foreach (var vendorInstitutionStatistics in detailsResponse.Response.ReconciliationDetails)
                {
                    var reconciliationDetailRecord = new InstitutionSummary
                    {
                        InstitutionId = vendorInstitutionStatistics.InstitutionId,
                        VendorId = request.VendorId,
                        VendorTotalPaymentAmount = vendorInstitutionStatistics.PaymentAmount,
                        VendorTotalPaymentCount = vendorInstitutionStatistics.PaymentCount,
                        VendorTotalCancelAmount = vendorInstitutionStatistics.CancellationAmount,
                        VendorTotalCancelCount = vendorInstitutionStatistics.CancellationCount,
                        ReconciliationDate = request.ReconciliationDate,
                        ReconciliationStatus = ReconciliationStatus.Fail,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                    };

                    var institutionsStatistic = institutionsStatistics.Find(s => s.InstitutionId == vendorInstitutionStatistics.InstitutionId);

                    if (institutionsStatistic != null)
                    {
                        reconciliationDetailRecord.TotalPaymentAmount = institutionsStatistic.PaymentAmount;
                        reconciliationDetailRecord.TotalPaymentCount = institutionsStatistic.PaymentCount;
                        reconciliationDetailRecord.TotalCancelAmount = institutionsStatistic.CancellationAmount;
                        reconciliationDetailRecord.TotalCancelCount = institutionsStatistic.CancellationCount;

                        if (reconciliationDetailRecord.VendorTotalPaymentAmount == reconciliationDetailRecord.TotalPaymentAmount
                            && reconciliationDetailRecord.VendorTotalPaymentCount == reconciliationDetailRecord.TotalPaymentCount
                            && reconciliationDetailRecord.VendorTotalCancelAmount == reconciliationDetailRecord.TotalCancelAmount
                            && reconciliationDetailRecord.VendorTotalCancelCount == reconciliationDetailRecord.TotalCancelCount)
                        {
                            reconciliationDetailRecord.ReconciliationStatus = ReconciliationStatus.Success;
                            vendorInstitutionStatistics.ReconciliationStatus = ReconciliationStatus.Success;
                        }

                        institutionsStatistics.Remove(institutionsStatistic);
                    }
                    else if (vendorInstitutionStatistics.PaymentCount == 0 && vendorInstitutionStatistics.CancellationCount == 0)
                    {
                        reconciliationDetailRecord.ReconciliationStatus = ReconciliationStatus.Success;
                        vendorInstitutionStatistics.ReconciliationStatus = ReconciliationStatus.Success;
                    }

                    await _reconciliationDetailRecordRepository.AddAsync(reconciliationDetailRecord);

                    vendorInstitutionStatistics.InstitutionSummaryId = reconciliationDetailRecord.Id;

                }

                foreach (var institutionsStatistic in institutionsStatistics)
                {
                    var reconciliationDetailRecord = new InstitutionSummary
                    {
                        InstitutionId = institutionsStatistic.InstitutionId,
                        VendorId = request.VendorId,
                        TotalPaymentAmount = institutionsStatistic.PaymentAmount,
                        TotalPaymentCount = institutionsStatistic.PaymentCount,
                        TotalCancelAmount = institutionsStatistic.CancellationAmount,
                        TotalCancelCount = institutionsStatistic.CancellationCount,
                        ReconciliationDate = request.ReconciliationDate,
                        ReconciliationStatus = ReconciliationStatus.Fail,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                    };

                    await _reconciliationDetailRecordRepository.AddAsync(reconciliationDetailRecord);

                    var mappedDetails = new InstitutionReconciliation
                    {
                        InstitutionSummaryId = reconciliationDetailRecord.Id,
                        InstitutionId = reconciliationDetailRecord.InstitutionId,
                        ReconciliationStatus = reconciliationDetailRecord.ReconciliationStatus,
                        CancellationAmount = institutionsStatistic.CancellationAmount,
                        PaymentAmount = institutionsStatistic.PaymentAmount,
                        CancellationCount = institutionsStatistic.CancellationCount,
                        PaymentCount = institutionsStatistic.PaymentCount,
                    };

                    detailsResponse.Response.ReconciliationDetails.Add(mappedDetails);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("ErrorPerformingReconciliationDetailOperation: {exception}", exception);
            }
        }

        return detailsResponse;
    }

    public async Task<BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse>> GetInstitutionPaymentDetailsAsync(Guid institutionSummaryId)
    {
        var institutionSummary = await _reconciliationDetailRecordRepository.GetByIdAsync(institutionSummaryId);

        if (institutionSummary == null)
        {
            throw new NotFoundException($"InstitutionSummaryRecordNotFound");
        }

        var request = new ReconcilliationInstitutionDetailRequest
        {
            VendorId = institutionSummary.VendorId,
            InstitutionId = institutionSummary.InstitutionId,
            ReconciliationDate = institutionSummary.ReconciliationDate
        };
        var vendorService = await GetVendorService(request.InstitutionId);
        var vendorApiResponse = await vendorService.GetInstitutionPaymentDetailsAsync(request);

        if (vendorApiResponse.IsSuccess)
        {
            var vendorSideTransactions = vendorApiResponse.Response.PaymentTransactions;
            var institutionTransactions = await _transactionService.GetReconciliableInstitutionTransactionsForVendor(request.InstitutionId, request.VendorId, request.ReconciliationDate);

            await SetOldInstitutionPaymentsPassive(institutionSummary.ReconciliationDate, institutionSummary.InstitutionId);

            foreach (var institutionTransaction in institutionTransactions)
            {
                var institutionRecord = new InstitutionDetail
                {
                    InstitutionSummaryId = institutionSummary.Id,
                    VendorId = institutionTransaction.VendorId,
                    InstitutionId = institutionTransaction.InstitutionId,
                    TransactionId = institutionTransaction.Id,
                    ReconciliationStatus = ReconciliationStatus.Fail,
                    PaymentStatus = PaymentStatus.MissingVendorTransaction,
                    ReconciliationDate = request.ReconciliationDate,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                };
                var vendorSideTransaction = vendorSideTransactions.Find(p => p.PaymentReferenceId == institutionTransaction.ReferenceId);

                if (vendorSideTransaction != null)
                {
                    institutionRecord.ReconciliationStatus = institutionTransaction.TransactionStatus == vendorSideTransaction.TransactionStatus
                        ? ReconciliationStatus.Success : ReconciliationStatus.Fail;
                    institutionRecord.BillNumber = vendorSideTransaction.BillNumber;
                    institutionRecord.BillAmount = vendorSideTransaction.BillAmount;
                    institutionRecord.BillCurrency = vendorSideTransaction.BillAmountCurrency;
                    institutionRecord.PaymentAmount = vendorSideTransaction.PaymentAmount;
                    institutionRecord.PaymentCurrency = vendorSideTransaction.PaymentAmountCurrency;
                    institutionRecord.PaymentReferenceId = vendorSideTransaction.PaymentReferenceId;
                    institutionRecord.PaymentStatus = PaymentStatus.ExistBothSides;
                    institutionRecord.BillDueDate = vendorSideTransaction.BillDueDate;
                    institutionRecord.BillDate = vendorSideTransaction.BillDate;

                    vendorSideTransactions.Remove(vendorSideTransaction);
                }
                else
                {
                    institutionRecord.BillNumber = institutionTransaction.BillNumber;
                    institutionRecord.BillAmount = institutionTransaction.BillAmount;
                    institutionRecord.BillCurrency = institutionTransaction.Currency;
                    institutionRecord.PaymentAmount = institutionTransaction.BillAmount;
                    institutionRecord.PaymentCurrency = institutionTransaction.Currency;
                    institutionRecord.PaymentReferenceId = institutionTransaction.ReferenceId;
                    institutionRecord.BillDueDate = institutionTransaction.BillDueDate;
                    institutionRecord.BillDate = institutionTransaction.BillDate;
                }

                await _reconciliationInstitutionRecordRepository.AddAsync(institutionRecord);
            }

            foreach (var vendorSideTransaction in vendorSideTransactions)
            {
                var institutionRecord = new InstitutionDetail
                {
                    InstitutionSummaryId = institutionSummary.Id,
                    InstitutionId = request.InstitutionId,
                    VendorId = request.VendorId,
                    ReconciliationStatus = ReconciliationStatus.Fail,
                    BillNumber = vendorSideTransaction.BillNumber,
                    BillAmount = vendorSideTransaction.BillAmount,
                    BillCurrency = vendorSideTransaction.BillAmountCurrency,
                    BillDate = vendorSideTransaction.BillDate,
                    BillDueDate = vendorSideTransaction.BillDueDate,
                    PaymentAmount = vendorSideTransaction.PaymentAmount,
                    PaymentCurrency = vendorSideTransaction.PaymentAmountCurrency,
                    PaymentReferenceId = vendorSideTransaction.PaymentReferenceId,
                    PaymentStatus = PaymentStatus.MissingTransaction,
                    ReconciliationDate = request.ReconciliationDate,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                };

                await _reconciliationInstitutionRecordRepository.AddAsync(institutionRecord);
            }
        }

        return vendorApiResponse;
    }

    public async Task<BillingResponse<InstitutionReconciliationCloseResponse>> CloseInstitutionReconciliationAsync(InstitutionReconciliationCloseRequest request)
    {
        var vendorService = await _billingVendorServiceFactory.GetBillingServiceAsync(request.VendorId);

        return await vendorService.InstitutionReconciliationCloseAsync(request);
    }

    private async Task InsertMissingReconciliationSummariesAsync(DateTime requestedDate, Guid vendorId)
    {
        var configuredPeriod = _configuration.GetValue<int>("ReconciliationPeriod");

        if (configuredPeriod == 0)
        {
            configuredPeriod = reconciliationPeriod;
        }

        var configuredDate = DateTime.Today.AddDays(-configuredPeriod).Date;

        try
        {
            var reconciliationSummaries = await _reconciliationRecordsRepository
                .GetAll()
                .Where(w => w.ReconciliationDate.Date < requestedDate.Date && w.ReconciliationDate.Date >= configuredDate)
                .ToListAsync();

            while (configuredDate < requestedDate.Date)
            {
                if (!reconciliationSummaries.Any(w => w.ReconciliationDate.Date == configuredDate.Date))
                {
                    var reconciliationSummary = new Summary
                    {
                        VendorId = vendorId,
                        TotalPaymentAmount = 0,
                        TotalPaymentCount = 0,
                        TotalCancelAmount = 0,
                        TotalCancelCount = 0,
                        VendorTotalPaymentAmount = 0,
                        VendorTotalPaymentCount = 0,
                        VendorTotalCancelAmount = 0,
                        VendorTotalCancelCount = 0,
                        ReconciliationDate = configuredDate,
                        ReconciliationStatus = ReconciliationStatus.Fail,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                    };

                    await _reconciliationRecordsRepository.AddAsync(reconciliationSummary);
                }

                configuredDate = configuredDate.AddDays(1).Date;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError("ErrorInsertintMissingReconciliationSummaries: {exception}", exception);
        }
    }

    public async Task<BillingResponse<bool>> DoReconciliationAsync(ReconciliationJobCommand request)
    {
        var response = new BillingResponse<bool>
        {
            IsSuccess = false,
            Response = false
        };

        await InsertMissingReconciliationSummariesAsync(request.ReconciliationDate, request.VendorId);

        try
        {
            _activeVendor = await _vendorRepository.GetByIdAsync(request.VendorId);

            var reconciliationSummaryResponse = await GetReconciliationSummaryAsync(new ReconciliationSummaryRequest
            {
                VendorId = request.VendorId,
                ReconciliationDate = request.ReconciliationDate
            });

            if (!reconciliationSummaryResponse.IsSuccess)
            {
                await NotifyReconciliationStatusAsync(request.ReconciliationDate, false, reconciliationSummaryResponse.ErrorMessage);

                _logger.LogError("ErrorGettingReconciliationSummaryForVendor: {VendorId}, Error: {ErrorMessage}", request.VendorId, reconciliationSummaryResponse.ErrorMessage);

                response.IsSuccess = true;

                return response;
            }

            var reconciliationDetailRequest = new ReconciliationDetailsRequest
            {
                VendorId = request.VendorId,
                ReconciliationDate = request.ReconciliationDate
            };

            var reconciliationDetailResponse = await GetReconciliationDetailsAsync(reconciliationDetailRequest);

            if (!reconciliationDetailResponse.IsSuccess)
            {
                await NotifyReconciliationStatusAsync(request.ReconciliationDate, false, reconciliationDetailResponse.ErrorMessage);

                _logger.LogError("ErrorGettingReconciliationDetailsForVendor: {VendorId}, Error: {ErrorMessage}", request.VendorId, reconciliationDetailResponse.ErrorMessage);

                response.IsSuccess = true;

                return response;
            }

            if (reconciliationSummaryResponse.Response?.ReconciliationStatus == ReconciliationStatus.Success)
            {
                var successfulInstitutions = reconciliationDetailResponse
                        .Response?
                        .ReconciliationDetails?
                        .Where(i => i.ReconciliationStatus == ReconciliationStatus.Success);

                if (successfulInstitutions.Any())
                {
                    var reconciliationCloseRequest = new InstitutionReconciliationCloseRequest
                    {
                        VendorId = request.VendorId,
                        InstitutionReconciliations = successfulInstitutions.ToList(),
                        ReconciliationDate = request.ReconciliationDate
                    };

                    var isReconciliationEnabled = await _billingVendorServiceFactory.IsReconciliationCloseNeededAsync(request.VendorId);

                    if (isReconciliationEnabled)
                    {
                        var reconciliationCloseResponse =
                            await CloseInstitutionReconciliationAsync(reconciliationCloseRequest);

                        if (reconciliationCloseResponse.IsSuccess)
                        {
                            var closedInstitutions = reconciliationCloseResponse.Response?.InstitutionReconciliations
                                ?.Where(w => w.ReconciliationStatus == ReconciliationStatus.Success)
                                .Select(s => s.InstitutionId)
                                .ToList();

                            if (closedInstitutions != null)
                            {
                                await SetOldInstitutionPaymentsPassive(request.ReconciliationDate, closedInstitutions);
                            }

                            var closeFailedInstitutions = reconciliationCloseResponse.Response
                                ?.InstitutionReconciliations
                                ?.Where(w => w.ReconciliationStatus == ReconciliationStatus.Fail)
                                .ToList();

                            foreach (var closeFailedInstitution in closeFailedInstitutions)
                            {
                                _logger.LogError("CouldNotCloseReconciliationDespiteSuccesfulResponse: {InstitutionId}", closeFailedInstitution.InstitutionId);
                            }
                        }
                    }
                }
            }

            var unsuccessfulInstitutions = reconciliationDetailResponse
                    .Response?
                    .ReconciliationDetails?
                    .Where(i => i.ReconciliationStatus == ReconciliationStatus.Fail);

            if (!unsuccessfulInstitutions.Any())
            {
                response.IsSuccess = true;
                response.Response = true;

                await NotifyReconciliationStatusAsync(request.ReconciliationDate, true, string.Empty);

                return response;
            }

            foreach (var unsuccessfulInstitution in unsuccessfulInstitutions)
            {
                await GetInstitutionPaymentDetailsAsync(unsuccessfulInstitution.InstitutionSummaryId);
            }

            response.IsSuccess = true;

            await NotifyReconciliationStatusAsync(request.ReconciliationDate, false, $"Unsuccessful Institution Count: {unsuccessfulInstitutions.Count()}");
        }
        catch (Exception exception)
        {
            await NotifyReconciliationStatusAsync(request.ReconciliationDate, false, exception.Message);

            response.IsSuccess = false;
            response.Response = false;

            _logger.LogError("ErrorPerformingReconciliationJobForVendor: {VendorId}, Error: {exception}", request.VendorId, exception);
        }

        return response;
    }

    private async Task<IBillingVendorService> GetVendorService(Guid institutionId)
    {
        _activeVendor = await _institutionService.GetActiveVendorIdByIdAsync(institutionId);

        return await _billingVendorServiceFactory.GetBillingServiceAsync(_activeVendor.Id);
    }

    private Transaction PopulateTransaction(PayInquiredBillCommand request)
    {
        var payeeFullName = request.PayeeFullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return new Transaction
        {
            InstitutionId = request.InstitutionId,
            ServiceRequestId = request.RequestId,
            BillAmount = request.Bill.Amount,
            CommissionAmount = request.Bill.CommissionAmount,
            Currency = request.Bill.Currency,
            BillId = request.Bill.Id,
            BillNumber = request.Bill.Number,
            SubscriptionNumber1 = request.Bill.SubscriberNumber1,
            SubscriptionNumber2 = request.Bill.SubscriberNumber2,
            SubscriptionNumber3 = request.Bill.SubscriberNumber3,
            BillDate = request.Bill.Date,
            BillDueDate = request.Bill.DueDate,
            PaymentDate = DateTime.Now,
            VendorId = _activeVendor.Id,
            PayeeFullName = string.Join(" ", payeeFullName),
            SubscriberName = request.Bill.SubscriberName,
            PayeeMobile = request.PayeeMobile,
            PayeeEmail = request.PayeeEmail,
            TransactionStatus = TransactionStatus.Error,
            UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
            WalletNumber = request.WalletNumber,
            AccountingReferenceId = Guid.NewGuid(),
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            BsmvAmount = request.Bill.BsmvAmount
        };
    }

    private static BillCancelRequest PopulateBillCancelRequest(Transaction transaction, string cancellationReason)
    {
        return new BillCancelRequest
        {
            InstitutionId = transaction.InstitutionId,
            RequestId = transaction.ServiceRequestId,
            VendorId = transaction.VendorId,
            Bill = new Bill
            {
                Id = transaction.BillId,
                Number = transaction.BillNumber,
                Date = transaction.BillDate,
                DueDate = transaction.BillDueDate,
                Currency = transaction.Currency,
                Amount = transaction.BillAmount,
                CommissionAmount = transaction.CommissionAmount,
                SubscriberName = transaction.SubscriberName,
                SubscriberNumber1 = transaction.SubscriptionNumber1,
                SubscriberNumber2 = transaction.SubscriptionNumber2,
                SubscriberNumber3 = transaction.SubscriptionNumber3,
            },
            CancellationReason = cancellationReason
        };
    }

    private TimeoutTransaction PopulateTimeoutTransaction(Transaction transaction)
    {
        return new TimeoutTransaction
        {
            VendorId = transaction.VendorId,
            InstitutionId = transaction.InstitutionId,
            BillId = transaction.BillId,
            BillNumber = transaction.BillNumber,
            BillAmount = transaction.BillAmount,
            BillDate = transaction.BillDate,
            BillDueDate = transaction.BillDueDate,
            Currency = transaction.Currency,
            SubscriptionNumber1 = transaction.SubscriptionNumber1,
            SubscriptionNumber2 = transaction.SubscriptionNumber2,
            SubscriptionNumber3 = transaction.SubscriptionNumber3,
            SubscriberName = transaction.SubscriberName,
            CommissionAmount = transaction.CommissionAmount,
            VoucherNumber = transaction.VoucherNumber,
            ReferenceId = transaction.ReferenceId,
            WalletNumber = transaction.WalletNumber,
            ServiceRequestId = transaction.ServiceRequestId,
            PaymentDate = transaction.PaymentDate,
            PayeeEmail = transaction.PayeeEmail,
            PayeeFullName = transaction.PayeeFullName,
            PayeeMobile = transaction.PayeeMobile,
            AccountingReferenceId = transaction.AccountingReferenceId,
            ProvisionReferenceId = transaction.ProvisionReferenceId,
            CreateDate = DateTime.Now,
            TransactionId = transaction.Id,
            UserId = transaction.UserId,
            NextTryTime = DateTime.Now.AddMinutes(1),
            RetryCount = 0,
            ErrorCode = transaction.ErrorCode,
            ErrorMessage = transaction.ErrorMessage,
            Description = transaction.Description,
            TimeoutTransactionStatus = TimeoutTransactionStatus.Pending,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
        };
    }

    private async Task SetOldInstitutionSummariesPassive(ReconciliationDetailsRequest request)
    {
        var oldInstitutionSummaries = _reconciliationDetailRecordRepository
            .GetAll()
            .Where(w => w.ReconciliationDate == request.ReconciliationDate
                && w.RecordStatus == RecordStatus.Active
            );

        if (request.InstitutionId != Guid.Empty)
        {
            oldInstitutionSummaries = oldInstitutionSummaries.Where(w => w.InstitutionId == request.InstitutionId);
        }

        await oldInstitutionSummaries
            .ExecuteUpdateAsync(u =>
                u.SetProperty(p =>
                    p.RecordStatus, RecordStatus.Passive
                )
            );
    }

    private async Task SetOldInstitutionPaymentsPassive(DateTime reconciliationDate, Guid institutionId)
    {
        await _reconciliationInstitutionRecordRepository
            .GetAll()
            .Where(w => w.ReconciliationDate == reconciliationDate
                && w.InstitutionId == institutionId
                && w.RecordStatus == RecordStatus.Active
            )
            .ExecuteUpdateAsync(u =>
                u.SetProperty(p =>
                    p.RecordStatus, RecordStatus.Passive
                )
            );
    }

    private async Task SetOldInstitutionPaymentsPassive(DateTime reconciliationDate, List<Guid> institutionIds)
    {
        var oldInstitutionPayments = await _reconciliationInstitutionRecordRepository
            .GetAll()
            .Where(w => w.ReconciliationDate == reconciliationDate
                && institutionIds.Contains(w.InstitutionId)
                && w.RecordStatus == RecordStatus.Active
            )
            .ToListAsync();

        foreach (var oldInstitutionPayment in oldInstitutionPayments)
        {
            oldInstitutionPayment.RecordStatus = RecordStatus.Passive;

            await _reconciliationInstitutionRecordRepository.UpdateAsync(oldInstitutionPayment);
        }
    }

    private async Task NotifyReconciliationStatusAsync(DateTime reconciliationDate, bool isSuccess, string errorMessage)
    {
        try
        {
            var notificationGroups = _configuration.GetSection("ReconciliationNotificationGroups").Get<List<string>>();
            var contactInformations = await _parameterService.GetParametersAsync("CompanyContactInformation");

            foreach (var notificationGroup in notificationGroups)
            {
                var contactInformation = contactInformations.Find(p => p.ParameterCode.Equals($"{notificationGroup}Email"));

                if (contactInformation != null)
                {
                    var sendEmail = new SendEmail
                    {
                        ToEmail = contactInformation.ParameterValue,
                        TemplateName = isSuccess ? "BillingReconciliationSuccess" : "BillingReconciliationFail",
                        DynamicTemplateData = new Dictionary<string, string>
                        {
                            { "vendor_name", _activeVendor.Name },
                            { "reconciliation_date", reconciliationDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)},
                            { "error_message", errorMessage }
                        }
                    };

                    using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Notification.SendEmail"));
                    await endpoint.Send(sendEmail, tokenSource.Token);

                }
            }
        }
        catch (Exception exception)
        {

            _logger.LogError("ErrorNotifyingReconciliationStatus: {exception}", exception);
        }
    }
}