using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Application.Commons.Models.BillingModels.Enums;
using LinkPara.Billing.Application.Commons.Models.ExternalServiceConfiguration;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;
using LinkPara.Billing.Application.Features.InstitutionApis.Queries.GetBillInquiry;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Interfaces;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.SekerBank.Models.Requests;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace LinkPara.Billing.Infrastructure.Services.Billing;

public class SekerBankBillingService : IBillingVendorService
{
    private readonly IGenericRepository<AuthorizationToken> _authorizationTokenRepository;
    private readonly ISekerBankApi _sekerBankApi;
    private readonly IVendorMapper _vendorMapper;
    private readonly ILogger<SekerBankBillingService> _logger;
    private readonly IInstitutionService _institutionService;
    private readonly IVaultClient _vaultClient;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IConfiguration _configuration;

    private readonly AuthorizationToken _authorizationToken;
    private readonly Vendor _vendor;
    private readonly int _authTokenInterval;

    public SekerBankBillingService(ILogger<SekerBankBillingService> logger,
        IGenericRepository<AuthorizationToken> authorizationTokenRepository,
        IVendorService vendorService,
        ISekerBankApi sekerBankApi,
        IVendorMapper vendorMapper,
        IInstitutionService institutionService,
        IVaultClient vaultClient,
        IApplicationUserService applicationUserService,
        IConfiguration configuration)
    {
        _logger = logger;
        _authorizationTokenRepository = authorizationTokenRepository;
        _sekerBankApi = sekerBankApi;
        _vendorMapper = vendorMapper;
        _vaultClient = vaultClient;
        _institutionService = institutionService;
        _applicationUserService = applicationUserService;
        _configuration = configuration;

        var serviceSettings = new ServiceSettings();

        _configuration.GetSection(nameof(ServiceSettings)).Bind(serviceSettings);
        _authTokenInterval = serviceSettings.SekerBankServiceSettings.AuthTokenInterval;

        _vendor = vendorService.GetByNameAsync("SekerBank").Result;
        _authorizationToken = GetSekerBankAuthorizationToken().Result;
    }

    public async Task<BillingResponse<BillCancelResponse>> CancelBillPaymentAsync(BillCancelRequest billCancelRequest)
    {
        var sekerBankInstitution = await _vendorMapper.GetVendorInstitutionByInstitutionIdAsync(billCancelRequest.InstitutionId, _vendor.Id);
        var requestCurrency = GetVendorCurrencyCode(billCancelRequest.Bill.Currency);
        var billToCancel = new SekerBankBillToCancel
        {
            billAmount = billCancelRequest.Bill.Amount,
            billAmountMoneyType = requestCurrency,
            billNo = billCancelRequest.Bill.Number,
            oid = billCancelRequest.Bill.Id,
            subscriberNo1 = billCancelRequest.Bill.SubscriberNumber1,
            subscriberNo2 = billCancelRequest.Bill.SubscriberNumber2,
            subscriberNo3 = billCancelRequest.Bill.SubscriberNumber3,
            cancelDescription = billCancelRequest.CancellationReason
        };

        billToCancel.billDueDate = billCancelRequest.Bill.DueDate.ToString("yyyyMMdd");
        billToCancel.billDate = billCancelRequest.Bill.Date == DateTime.MinValue ?
            null : billCancelRequest.Bill.Date?.ToString("yyyyMMdd");

        var sekerBankBillCancelRequest = new SekerBankBillPaymentCancelRequest
        {
            institutionOid = sekerBankInstitution.VendorInstitutionId,
            requestId = billCancelRequest.RequestId,
            billsToCancel = new List<SekerBankBillToCancel> { billToCancel }
        };

        var sekerBankResponse = await _sekerBankApi.CancelBillPaymentAsync(_authorizationToken.AccessToken, sekerBankBillCancelRequest);
        var response = new BillingResponse<BillCancelResponse>
        {
            IsSuccess = sekerBankResponse.IsSuccess
        };

        if (sekerBankResponse.IsSuccess)
        {
            var invoice = sekerBankResponse.Data.billsToCancel.FirstOrDefault();
            var responseCurrency = GetIsoCurrencyCode(invoice?.billAmountMoneyType);
            var parsedInvoice = new BillCancelInvoice
            {
                Amount = invoice.billAmount,
                Id = invoice.oid,
                Number = invoice.billNo,
                Currency = responseCurrency,
                ReferenceId = invoice.referenceId,
                VoucherNumber = invoice.voucherNo,
                Description = invoice.cancelDescription,
                IsSuccess = invoice.cancelSuccess == "1",
                ResultDescription = invoice.cancelResult,
                SubscriberNumber1 = invoice.subscriberNo1
            };

            parsedInvoice.CancelDate = ParseDate(invoice.cancelDate);
            parsedInvoice.DueDate = ParseDate(invoice.billDueDate);
            parsedInvoice.Date = ParseDate(invoice.billDate);

            response.Response = new BillCancelResponse
            {
                BillCancelInvoice = parsedInvoice,
                InstitutionId = billCancelRequest.InstitutionId,
                VendorId = _vendor.Id,
                RequestId = billCancelRequest.RequestId
            };
        }
        else
        {
            response.ErrorCode = sekerBankResponse.ErrorResponse.errorCode;
            response.ErrorMessage = sekerBankResponse.ErrorResponse.errorMessage;
        }

        return response;
    }

    public async Task<BillingResponse<ReconciliationSummaryResponse>> GetReconciliationSummaryAsync(TransactionStatistics statistics)
    {
        var response = new BillingResponse<ReconciliationSummaryResponse> { IsSuccess = false };

        try
        {
            var sekerBankReconcilationSummaryRequest = new SekerBankReconciliationSummaryRequest
            {
                reconciliationDate = statistics.StatisticsDate.ToString("yyyMMdd"),
                totalCancelationAmount = statistics.CancellationAmount,
                totalCancelationCount = statistics.CancellationCount,
                totalPaymentAmount = statistics.PaymentAmount,
                totalPaymentCount = statistics.PaymentCount
            };

            var sekerBankResponse = await _sekerBankApi.GetReconciliationSummaryAsync(_authorizationToken.AccessToken, sekerBankReconcilationSummaryRequest);

            response.IsSuccess = sekerBankResponse.IsSuccess;

            if (response.IsSuccess)
            {
                response.Response = new ReconciliationSummaryResponse
                {
                    CancellationAmount = sekerBankResponse.Data.totalCancelationAmount,
                    PaymentAmount = sekerBankResponse.Data.totalPaymentAmount,
                    CancellationCount = sekerBankResponse.Data.totalCancelationCount,
                    PaymentCount = sekerBankResponse.Data.totalPaymentCount,
                    ReconciliationDate = ParseDate(sekerBankResponse.Data.reconciliationDate),
                    ReconciliationStatus = sekerBankResponse.Data.reconciliationStatus == "1"
                                        ? ReconciliationStatus.Success : ReconciliationStatus.Fail
                };
            }
            else
            {
                response.ErrorCode = sekerBankResponse.ErrorResponse.errorCode;
                response.ErrorMessage = sekerBankResponse.ErrorResponse.errorMessage;
            }
        }
        catch (ReconciliationException reconciliationException)
        {
            response.ErrorMessage = reconciliationException.Message;
            response.ErrorCode = reconciliationException.Code;
        }

        return response;
    }

    public async Task<List<SectorMapping>> GetSectorListAsync()
    {
        var sekerBankResponse = await _sekerBankApi.GetInstitutionListAsync(_authorizationToken.AccessToken);

        if (!sekerBankResponse.IsSuccess)
        {
            var errorMessage = $"ErrorGettingSekerBankInstitutionList: ErrorCode: {sekerBankResponse.ErrorResponse.errorCode}, ErrorMessage: {sekerBankResponse.ErrorResponse.errorMessage}";

            _logger.LogError("An error occurred: {ErrorMessage}", errorMessage);


            throw new ExternalApiException(_vendor, errorMessage);
        }

        return sekerBankResponse.Data
            .Select(s => new SectorMapping
            {
                VendorId = _vendor.Id,
                VendorSectorId = s.sectorName
            })
            .DistinctBy(s => s.VendorSectorId)
            .ToList();
    }

    public async Task<List<InstitutionMapping>> GetInstitutionListAsync()
    {
        var sekerBankResponse = await _sekerBankApi.GetInstitutionListAsync(_authorizationToken.AccessToken);

        if (!sekerBankResponse.IsSuccess)
        {
            var errorMessage = $"ErrorGettingSekerBankInstitutionList: ErrorCode: {sekerBankResponse.ErrorResponse.errorCode}, ErrorMessage: {sekerBankResponse.ErrorResponse.errorMessage}";

            _logger.LogError("An error occurred: {ErrorMessage}", errorMessage);


            throw new ExternalApiException(_vendor, errorMessage);
        }

        var institutionMappings = new List<InstitutionMapping>();

        foreach (var sekerBankInstitution in sekerBankResponse.Data)
        {
            try
            {
                var operationMode = sekerBankInstitution.operationMode switch
                {
                    "Online" => OperationMode.Online,
                    "Offline" => OperationMode.Offline,
                    _ => OperationMode.None
                };

                var institutionMapping = new InstitutionMapping
                {
                    VendorId = _vendor.Id,
                    VendorInstitutionId = sekerBankInstitution.institutionOid,
                    Institution = new Institution
                    {
                        Name = sekerBankInstitution.institutionName,
                        FieldRequirementType = sekerBankInstitution.subscriberNoRequirement == "JUST_ONE" ? FieldRequirementType.JustOne : FieldRequirementType.All,
                        Fields = sekerBankInstitution.subscriberNoDetails.Select(f => new Field
                        {
                            Label = f.lable,
                            Length = int.Parse(f.lenght),
                            Mask = f.mask,
                            Order = int.Parse(f.order),
                            Pattern = f.pattern,
                            Placeholder = f.example,
                            Prefix = f.prefix,
                            Suffix = f.suffix
                        }).ToList(),
                        OperationMode = operationMode,
                        Sector = new Sector
                        {
                            Name = sekerBankInstitution.sectorName
                        }
                    }
                };

                institutionMappings.Add(institutionMapping);
            }
            catch (Exception exception)
            {
                _logger.LogError("ErrorMappingSekerBankInstitution: {sekerBankInstitution}, Error: {exception}", sekerBankInstitution, exception);
            }
        }

        return institutionMappings;
    }

    public async Task<List<Institution>> GetInstitutionListBySectorAsync(Guid sectorId)
    {
        var sekerBankSector = await _vendorMapper.GetVendorSectorBySectorIdAsync(sectorId, _vendor.Id);
        var sekerBankResponse = await _sekerBankApi.GetInstitutionListAsync(_authorizationToken.AccessToken);

        if (!sekerBankResponse.IsSuccess)
        {
            var errorMessage = $"ErrorGettingSekerBankInstitutionList: ErrorCode: {sekerBankResponse.ErrorResponse.errorCode}, ErrorMessage: {sekerBankResponse.ErrorResponse.errorMessage}";

            _logger.LogError("An error occurred: {ErrorMessage}", errorMessage);


            throw new ExternalApiException(_vendor, errorMessage);
        }

        var institutions = new List<Institution>();

        foreach (var institution in sekerBankResponse.Data)
        {
            if (institution.sectorName == sekerBankSector.VendorSectorId)
            {
                institutions.Add(new Institution
                {
                    Name = institution.institutionName,
                    Sector = sekerBankSector.Sector,
                    OperationMode = (OperationMode)Enum.Parse(typeof(OperationMode), institution.operationMode),
                    FieldRequirementType = institution.subscriberNoRequirement == "JUST_ONE" ? FieldRequirementType.JustOne : FieldRequirementType.All,
                    Fields = institution.subscriberNoDetails.Select(f => new Field
                    {
                        Label = f.lable,
                        Length = int.Parse(f.lenght),
                        Mask = f.mask,
                        Order = int.Parse(f.order),
                        Pattern = f.pattern,
                        Placeholder = f.example,
                        Prefix = f.prefix,
                        Suffix = f.suffix
                    }).ToList()
                });
            }
        }

        return institutions;
    }

    public async Task<BillingResponse<ReconciliationDetailsResponse>> GetReconciliationDetailsAsync(ReconciliationDetailsRequest reconcilationDetailsRequest)
    {
        var response = new BillingResponse<ReconciliationDetailsResponse> { IsSuccess = false };

        try
        {
            var sekerBankReconcilationDetailsRequest = new SekerBankReconciliationDetailsRequest
            {
                reconciliationDate = reconcilationDetailsRequest.ReconciliationDate.ToString("yyyMMdd"),
                institutionOid = string.Empty
            };

            if (reconcilationDetailsRequest.InstitutionId != Guid.Empty)
            {
                var sekerBankInstitution = await _vendorMapper.GetVendorInstitutionByInstitutionIdAsync(reconcilationDetailsRequest.InstitutionId, _vendor.Id);

                sekerBankReconcilationDetailsRequest.institutionOid = sekerBankInstitution.VendorInstitutionId;

                var sekerBankResponse = await _sekerBankApi.GetInstitutionReconciliationDetailsAsync(_authorizationToken.AccessToken, sekerBankReconcilationDetailsRequest);

                response.IsSuccess = sekerBankResponse.IsSuccess;

                if (response.IsSuccess)
                {
                    var reconciliationDetails = sekerBankResponse.Data;
                    response.Response = new ReconciliationDetailsResponse
                    {
                        ReconciliationDate = ParseDate(reconciliationDetails.reconciliationDate),
                        ReconciliationDetails = new List<InstitutionReconciliation>
                        {
                            new InstitutionReconciliation
                            {
                                CancellationAmount = reconciliationDetails.detail?.totalCancelationAmount ?? 0,
                                CancellationCount = reconciliationDetails.detail?.totalCancelationCount ?? 0,
                                PaymentAmount = reconciliationDetails.detail?.totalPaymentAmount ?? 0,
                                PaymentCount = reconciliationDetails.detail?.totalPaymentCount ?? 0,
                                InstitutionId = reconcilationDetailsRequest.InstitutionId,
                                ReconciliationStatus = ReconciliationStatus.Fail
                            }
                        }
                    };
                }
                else
                {
                    response.ErrorCode = sekerBankResponse.ErrorResponse.errorCode;
                    response.ErrorMessage = sekerBankResponse.ErrorResponse.errorMessage;
                }
            }
            else
            {
                var sekerBankResponse = await _sekerBankApi.GetReconciliationDetailsAsync(_authorizationToken.AccessToken, sekerBankReconcilationDetailsRequest);

                response.IsSuccess = sekerBankResponse.IsSuccess;

                if (response.IsSuccess)
                {
                    var reconciliationDetails = sekerBankResponse.Data;
                    var institutionMappings = await _vendorMapper.GetInstitutionMappingsByVendorAsync(_vendor.Id);

                    response.Response = new ReconciliationDetailsResponse
                    {
                        ReconciliationDate = ParseDate(reconciliationDetails.reconciliationDate),
                        ReconciliationDetails = reconciliationDetails.details
                        .Select(d => new InstitutionReconciliation
                        {
                            CancellationAmount = d?.totalCancelationAmount ?? 0,
                            CancellationCount = d?.totalCancelationCount ?? 0,
                            PaymentAmount = d?.totalPaymentAmount ?? 0,
                            PaymentCount = d?.totalPaymentCount ?? 0,
                            InstitutionId = institutionMappings.Find(i => i.VendorInstitutionId == d.institutionOid) != null ?
                            institutionMappings.Find(i => i.VendorInstitutionId == d.institutionOid).InstitutionId : Guid.Empty,
                            ReconciliationStatus = ReconciliationStatus.Fail
                        })
                        .ToList()
                    };
                }
                else
                {
                    response.ErrorCode = sekerBankResponse.ErrorResponse.errorCode;
                    response.ErrorMessage = sekerBankResponse.ErrorResponse.errorMessage;
                }
            }
        }
        catch (ReconciliationException reconciliationException)
        {
            response.ErrorMessage = reconciliationException.Message;
            response.ErrorCode = reconciliationException.Code;
        }

        return response;
    }

    public async Task<BillingResponse<BillInquiryResponse>> InquireBillsAsync(InquireBillQuery billInquiryRequest)
    {
        var sekerBankInstitution = await _vendorMapper.GetVendorInstitutionByInstitutionIdAsync(billInquiryRequest.InstitutionId, _vendor.Id);
        var sekerBankBillInquiryRequest = new SekerBankBillInquiryRequest
        {
            institutionOid = sekerBankInstitution.VendorInstitutionId,
            subscriberNo1 = billInquiryRequest.SubscriberNumber1,
            subscriberNo2 = billInquiryRequest.SubscriberNumber2,
            subscriberNo3 = billInquiryRequest.SubscriberNumber3
        };
        var sekerBankResponse = await _sekerBankApi.InquireBillsAsync(_authorizationToken.AccessToken, sekerBankBillInquiryRequest);
        var response = new BillingResponse<BillInquiryResponse>
        {
            IsSuccess = sekerBankResponse.IsSuccess
        };

        if (sekerBankResponse.IsSuccess)
        {
            var sekerBankBillInquiryResponse = sekerBankResponse.Data;
            var bills = new List<Bill>();

            foreach (var debt in sekerBankBillInquiryResponse.debts)
            {
                var billDate = ParseDate(debt.billDate);
                var billDueDate = ParseDate(debt.billDueDate);

                var billCurrency = GetIsoCurrencyCode(debt.billAmountMoneyType);

                bills.Add(new Bill
                {
                    Amount = debt.billAmount,
                    Currency = billCurrency,
                    Date = billDate,
                    DueDate = billDueDate,
                    Number = debt.billNo,
                    Id = debt.oid,
                    SubscriberName = debt.subscriberName,
                    SubscriberNumber1 = debt.subscriberNo1,
                    SubscriberNumber2 = debt.subscriberNo2,
                    SubscriberNumber3 = debt.subscriberNo3
                });
            }

            response.Response = new BillInquiryResponse
            {
                RequestId = sekerBankBillInquiryResponse.requestId,
                InstitutionId = billInquiryRequest.InstitutionId,
                VendorId = _vendor.Id,
                Bills = bills
            };
        }
        else
        {
            response.ErrorCode = sekerBankResponse.ErrorResponse.errorCode;
            response.ErrorMessage = sekerBankResponse.ErrorResponse.errorMessage;
        }

        return response;
    }

    public async Task<BillingResponse<BillStatusResponse>> InquireBillStatusAsync(BillStatusRequest billStatusRequest)
    {
        var institution = await _vendorMapper.GetVendorInstitutionByInstitutionIdAsync(billStatusRequest.InstitutionId, billStatusRequest.VendorId);
        var sekerBankBillStatusRequest = new SekerBankBillStatusRequest
        {
            institutionOid = institution.VendorInstitutionId,
            requestId = billStatusRequest.RequestId,
            billOid = billStatusRequest.BillId
        };
        var sekerBankResponse = await _sekerBankApi.InquireBillStatusAsync(_authorizationToken.AccessToken, sekerBankBillStatusRequest);
        var response = new BillingResponse<BillStatusResponse>
        {
            IsSuccess = sekerBankResponse.IsSuccess
        };

        if (sekerBankResponse.IsSuccess)
        {
            var billStatus = sekerBankResponse.Data;
            var status = billStatus.billStatus switch
            {
                "0" => BillStatus.PaymentCancelled,
                "1" => BillStatus.Paid,
                "2" => BillStatus.AwaitingCancelConfirmation,
                "3" => BillStatus.AwaitingPaymentConfirmation,
                _ => throw new InvalidBillStatusException(_vendor, $"InvalidBillStatusValue: {billStatus.billStatus}")
            };

            response.Response = new BillStatusResponse
            {
                InstitutionId = billStatusRequest.InstitutionId,
                DebtOid = billStatus.bpcDebtOid,
                BpcOid = billStatus.bpcOid,
                BillStatus = status
            };
        }
        else
        {
            response.ErrorCode = sekerBankResponse.ErrorResponse.errorCode;
            response.ErrorMessage = sekerBankResponse.ErrorResponse.errorMessage;
        }

        return response;
    }

    public async Task<BillingResponse<BillPaymentResponse>> PayInquiredBillsAsync(PayInquiredBillCommand billPaymentRequest)
    {
        var sekerBankInstitutionMapping = await _vendorMapper.GetVendorInstitutionByInstitutionIdAsync(billPaymentRequest.InstitutionId, _vendor.Id);

        var billCurrency = GetVendorCurrencyCode(billPaymentRequest.Bill.Currency);
        var bill = new SekerBankBill
        {
            billNo = billPaymentRequest.Bill.Number,
            oid = billPaymentRequest.Bill.Id,
            billAmount = billPaymentRequest.Bill.Amount,
            billAmountMoneyType = billCurrency,
            billDueDate = billPaymentRequest.Bill.DueDate.ToString("yyyyMMdd"),
            subscriberNo1 = billPaymentRequest.Bill.SubscriberNumber1,
            subscriberNo2 = billPaymentRequest.Bill.SubscriberNumber2,
            subscriberNo3 = billPaymentRequest.Bill.SubscriberNumber3
        };

        bill.billDate = billPaymentRequest.Bill.Date == DateTime.MinValue ?
            null : billPaymentRequest.Bill?.Date?.ToString("yyyyMMdd");

        var paymentRequest = new SekerBankBillPaymentRequest
        {
            requestId = billPaymentRequest.RequestId,
            institutionOid = sekerBankInstitutionMapping.VendorInstitutionId,
            billsToPay = new List<SekerBankBill> { bill }
        };

        var sekerBankResponse = await _sekerBankApi.PayInquiredBillsAsync(_authorizationToken.AccessToken, paymentRequest);
        var response = new BillingResponse<BillPaymentResponse>
        {
            IsSuccess = sekerBankResponse.IsSuccess
        };

        if (sekerBankResponse.IsSuccess)
        {
            var payment = sekerBankResponse.Data;
            var invoice = payment.billsToPay.FirstOrDefault();
            var isoCurrencyCode = GetIsoCurrencyCode(invoice.billAmountMoneyType);

            var billDate = ParseDate(invoice.billDate);
            var billDueDate = ParseDate(invoice.billDueDate);

            response.Response = new BillPaymentResponse
            {
                InstitutionId = billPaymentRequest.InstitutionId,
                RequestId = billPaymentRequest.RequestId,
                VendorId = _vendor.Id,
                Invoice = new BillInvoice
                {
                    Id = invoice.oid,
                    Number = invoice.billNo,
                    Amount = invoice.billAmount,
                    VoucherNumber = invoice.voucherNo,
                    Date = billDate,
                    DueDate = billDueDate,
                    Description = invoice.resultDesc,
                    ReferenceId = invoice.referenceId,
                    IsSuccess = invoice.resultCode == "1",
                    Currency = isoCurrencyCode,
                    SubscriberNumber1 = invoice.subscriberNo1,
                }
            };
        }
        else
        {
            response.ErrorCode = sekerBankResponse.ErrorResponse.errorCode;
            response.ErrorMessage = sekerBankResponse.ErrorResponse.errorMessage;

        }

        return response;
    }

    public async Task<BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse>> GetInstitutionPaymentDetailsAsync(ReconcilliationInstitutionDetailRequest reconciliationDetailsRequest)
    {
        var sekerBankInstitutionMapping = await _vendorMapper.GetVendorInstitutionByInstitutionIdAsync(reconciliationDetailsRequest.InstitutionId, _vendor.Id);
        var paymentDetailsRequest = new SekerBankReconciliationDetailsRequest
        {
            institutionOid = sekerBankInstitutionMapping.VendorInstitutionId,
            reconciliationDate = reconciliationDetailsRequest.ReconciliationDate.ToString("yyyyMMdd")
        };
        var response = new BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse> { IsSuccess = false };

        try
        {
            var sekerBankResponse = await _sekerBankApi.GetInstitutionPaymentDetailsAsync(_authorizationToken.AccessToken, paymentDetailsRequest);

            response.IsSuccess = sekerBankResponse.IsSuccess;

            if (response.IsSuccess)
            {
                var paymentDetailsResponse = sekerBankResponse.Data;
                var payments = paymentDetailsResponse.payments
                    .Select(p => new PaymentTransaction
                    {
                        BillAmount = p.billAmount,
                        BillAmountCurrency = GetIsoCurrencyCode(p.billAmountMoneyType),
                        PaymentAmount = p.paymentAmount,
                        PaymentAmountCurrency = GetIsoCurrencyCode(p.billAmountMoneyType),
                        BillDate = ParseDate(p.billDate),
                        BillDueDate = ParseDate(p.billDueDate),
                        BillNumber = p.billNo,
                        PaymentReferenceId = p.paymentReferenceId,
                        SubscriberNumber1 = p.subscriberNo1,
                        SubscriberNumber2 = p.subscriberNo2,
                        SubscriberNumber3 = p.subscriberNo3,
                        TransactionStatus = TransactionStatus.Paid
                    })
                    .ToList();

                payments.AddRange(paymentDetailsResponse.canceleds
                        .Select(p => new PaymentTransaction
                        {
                            BillAmount = p.billAmount,
                            BillAmountCurrency = GetIsoCurrencyCode(p.billAmountMoneyType),
                            PaymentAmount = p.paymentAmount,
                            PaymentAmountCurrency = GetIsoCurrencyCode(p.billAmountMoneyType),
                            BillDate = ParseDate(p.billDate),
                            BillDueDate = ParseDate(p.billDueDate),
                            BillNumber = p.billNo,
                            PaymentReferenceId = p.paymentReferenceId,
                            SubscriberNumber1 = p.subscriberNo1,
                            SubscriberNumber2 = p.subscriberNo2,
                            SubscriberNumber3 = p.subscriberNo3,
                            TransactionStatus = TransactionStatus.Cancelled
                        })
                        .ToList());

                response.Response = new ReconciliationInstitutionPaymentDetailsSummaryResponse
                {
                    InstitutionId = reconciliationDetailsRequest.InstitutionId,
                    PaymentAmount = paymentDetailsResponse.totalPaymentAmount,
                    PaymentCount = paymentDetailsResponse.totalPaymentCount,
                    CancellationAmount = paymentDetailsResponse.totalCancelationAmount,
                    CancellationCount = paymentDetailsResponse.totalPaymentCount,
                    PaymentTransactions = payments
                };
            }
            else
            {
                response.ErrorCode = sekerBankResponse.ErrorResponse.errorCode;
                response.ErrorMessage = sekerBankResponse.ErrorResponse.errorMessage;
            }
        }
        catch (ReconciliationException reconciliationException)
        {
            response.ErrorCode = reconciliationException.Code;
            response.ErrorMessage = reconciliationException.Message;
        }

        return response;
    }

    public async Task<BillingResponse<InstitutionReconciliationCloseResponse>> InstitutionReconciliationCloseAsync(InstitutionReconciliationCloseRequest institutionReconciliationCloseRequest)
    {
        var response = new BillingResponse<InstitutionReconciliationCloseResponse> { IsSuccess = false };

        try
        {
            var sekerBankRequest = new SekerBankInquireInstitutionReconciliationRequest
            {
                reconciliationDate = institutionReconciliationCloseRequest.ReconciliationDate.ToString("yyyyMMdd"),
                details = institutionReconciliationCloseRequest.InstitutionReconciliations
               .Select(i => new SekerBankInstitutionReconciliation
               {
                   totalCancelationAmount = i.CancellationAmount,
                   totalPaymentAmount = i.PaymentAmount,
                   institutionOid = _vendorMapper.GetVendorInstitutionByInstitutionIdAsync(i.InstitutionId, _vendor.Id).Result.VendorInstitutionId,
                   totalCancelationCount = i.CancellationCount,
                   totalPaymentCount = i.PaymentCount
               })
               .ToList()
            };
            var sekerBankResponse = await _sekerBankApi.InquireInstitutionReconciliationStatusAsync(_authorizationToken.AccessToken, sekerBankRequest);

            response.IsSuccess = sekerBankResponse.IsSuccess;

            if (response.IsSuccess)
            {
                response.Response = new InstitutionReconciliationCloseResponse
                {
                    ReconciliationDate = ParseDate(sekerBankResponse.Data.reconciliationDate),
                    InstitutionReconciliations = sekerBankResponse.Data.details
                    .Select(i => new InstitutionReconciliationCloseDetails
                    {
                        CancellationAmount = i.totalCancelationAmount,
                        CancellationCount = i.totalCancelationCount,
                        PaymentAmount = i.totalPaymentAmount,
                        PaymentCount = i.totalPaymentCount,
                        ReconciliationDate = ParseDate(i.reconciliationDate),
                        ReconciliationStatus = i.reconciliationStatus == "1" ? ReconciliationStatus.Success : ReconciliationStatus.Fail,
                        InstitutionId = _vendorMapper.GetInstitutionByVendorInstitutionIdAsync(i.institutionOid, _vendor.Id).Result.Id
                    })
                    .ToList()
                };
            }
            else
            {
                response.ErrorCode = sekerBankResponse.ErrorResponse.errorCode;
                response.ErrorMessage = sekerBankResponse.ErrorResponse.errorMessage;
            }
        }
        catch (ReconciliationException reconciliationException)
        {
            response.ErrorCode = reconciliationException.Code;
            response.ErrorMessage = reconciliationException.Message;
        }

        return response;
    }

    private async Task<AuthorizationToken> GetSekerBankAuthorizationToken()
    {
        var authorizationToken = await _authorizationTokenRepository.GetAll()
                .FirstOrDefaultAsync(t => t.VendorId == _vendor.Id
                            && t.ExpiryDate >= DateTime.Now.AddSeconds(_authTokenInterval));

        if (authorizationToken == null)
        {
            var serviceSettings = await _vaultClient.GetSecretValueAsync<SekerBankServiceSettings>("BillingSecrets", "SekerBank");
            var loginResponse = await _sekerBankApi.LoginAsync(new SekerBankAuthorizationRequest
            {
                username = serviceSettings.Username,
                password = serviceSettings.Password,
                grant_type = "password"
            });

            if (!loginResponse.IsSuccess)
            {
                _logger.LogError(
                    "ErrorGettingSekerBankAuthorizationToken: ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
                    loginResponse.ErrorResponse.errorCode,
                    loginResponse.ErrorResponse.errorMessage
 );


                throw new ExternalApiException(_vendor, $"ErrorAuthorizingSekerBank");
            }

            authorizationToken = new AuthorizationToken
            {
                AccessToken = loginResponse.Data.access_token,
                RefreshToken = loginResponse.Data.refresh_token,
                VendorId = _vendor.Id,
                TokenType = loginResponse.Data.token_type,
                RegisterDate = loginResponse.Data.register_date,
                ExpiryDate = DateTime.Now.AddSeconds(loginResponse.Data.expires_in),
                CreatedBy = _applicationUserService.ApplicationUserId.ToString()
            };

            await _authorizationTokenRepository.AddAsync(authorizationToken);
        }

        return authorizationToken;
    }

    public async Task ValidateRequestAsync(Guid institutionId, string subscriberNumber1, string subscriberNumber2, string subscriberNumber3)
    {
        var institution = await _institutionService.GetByIdAsync(institutionId);

        if (!institution.Fields.Any())
        {
            return;
        }

        var requirementType = institution.FieldRequirementType;
        var fields = institution.Fields.OrderBy(f => f.Order).ToList();

        if (!ValidatePattern(subscriberNumber1, fields[0]))
        {
            throw new InvalidInputException();
        }

        if (requirementType == FieldRequirementType.All)
        {
            if (fields.Count > 1 && !ValidatePattern(subscriberNumber2, fields[1]))
            {
                throw new InvalidInputException();
            }

            if (fields.Count > 2 && !ValidatePattern(subscriberNumber3, fields[2]))
            {
                throw new InvalidInputException();
            }
        }
    }

    private static string GetIsoCurrencyCode(string currencyCode)
    {
        return currencyCode switch
        {
            "TL" => "TRY",
            "USD" => "USD",
            "EUR" => "EUR",
            _ => throw new ArgumentException($"UnknownCurrencyCode: {currencyCode}")
        };
    }

    private static string GetVendorCurrencyCode(string currencyCode)
    {
        return currencyCode switch
        {
            "TRY" => "TL",
            "USD" => "USD",
            "EUR" => "EUR",
            _ => throw new ArgumentException($"UnknownCurrencyCode: {currencyCode}")
        };
    }

    private static bool ValidatePattern(string subscriberNumber, Field field)
    {
        if (subscriberNumber.Length != field.Length)
        {
            return false;
        }

        var pattern = field.Pattern;

        for (int i = 0; i < pattern.Length; i++)
        {
            var character = subscriberNumber[i];
            var code = pattern[i];

            if (code == '*')
            {
                continue;
            }

            if (
                   (code == '#' && !char.IsDigit(character))
                || (code == 'X' && !char.IsLetter(character))
                || (code == 'Q' && !char.IsLetterOrDigit(character))
                || (code == '.' && character != '.')
            )
            {
                return false;
            }
        }

        return true;
    }

    private static DateTime ParseDate(string dateString)
    {
        const string dateFormat = "yyyyMMdd";

        if (string.IsNullOrEmpty(dateString))
        {
            return DateTime.MinValue;
        }

        if (dateString.Length > dateFormat.Length)
        {
            dateString = dateString.Substring(0, dateFormat.Length);
        }

        return DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture);
    }
}