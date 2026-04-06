using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Application.Commons.Models.BillingModels.Enums;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;
using LinkPara.Billing.Application.Features.InstitutionApis.Queries.GetBillInquiry;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using System.Globalization;
using LinkPara.Security;

namespace LinkPara.Billing.Infrastructure.Services.BillingServices;

public class MockBillingService : IBillingVendorService
{
    private readonly Vendor _vendor;
    private readonly IVendorMapper _vendorMapper;
    private readonly ISecureRandomGenerator _randomGenerator;

    public MockBillingService(IVendorService vendorService,
        IVendorMapper vendorMapper,
        ISecureRandomGenerator randomGenerator)
    {
        _vendor = vendorService.GetByNameAsync("MockBank").Result;
        _vendorMapper = vendorMapper;
        _randomGenerator = randomGenerator;
    }

    public async Task<BillingResponse<BillCancelResponse>> CancelBillPaymentAsync(BillCancelRequest billCancelRequest)
    {
        var convertedDateTime = DateTime.Now.ToString("yyyyMMdd");
        var responseCurrency = GetIsoCurrencyCode("TL");
        var parsedInvoice = new BillCancelInvoice
        {
            Amount = 30,
            Id = "3d00000000D000L1",
            Number = "00000001",
            Currency = responseCurrency,
            ReferenceId = "0000000001",
            VoucherNumber = "0000001",
            Description = "Cancelled",
            IsSuccess = true,
            ResultDescription = "Cancelled",
            SubscriberNumber1 = "00000001",
            CancelDate = ParseDate(convertedDateTime),
            DueDate = ParseDate(convertedDateTime),
            Date = ParseDate(convertedDateTime)
        };

        var response = new BillingResponse<BillCancelResponse>
        {
            IsSuccess = true,
            Response = new BillCancelResponse
            {
                BillCancelInvoice = parsedInvoice,
                InstitutionId = billCancelRequest.InstitutionId,
                VendorId = _vendor.Id,
                RequestId = billCancelRequest.RequestId
            }
        };

        return response;
    }

    public async Task<List<InstitutionMapping>> GetInstitutionListAsync()
    {
        return new List<InstitutionMapping>
        {
            new()
            {
                VendorId = _vendor.Id,
                VendorInstitutionId = "MockInstitutionId",
                Institution = new Institution
                {
                    Name = "MockInstitution",
                    FieldRequirementType = FieldRequirementType.JustOne,
                    Fields = new List<Field>{
                            new ()
                            {
                                Label = "Abone No",
                                Length = 10,
                                Mask = "##########",
                                Order = 1,
                                Pattern = "##########",
                                Placeholder = "1234567890",
                                Prefix = "1",
                                Suffix = "1"
                            }
                        },
                    OperationMode = OperationMode.Online,
                    Sector = new Sector
                    {
                        Name = "MockSector",
                    }
            }
            }
        };
    }

    public async Task<List<Institution>> GetInstitutionListBySectorAsync(Guid sectorId)
    {
        var mockBankSector = await _vendorMapper.GetVendorSectorBySectorIdAsync(sectorId, _vendor.Id);

        return new List<Institution>
        {
            new()
            {
                 Name = "MockInstitution",
                 Sector = mockBankSector.Sector ?? null,
                 OperationMode = OperationMode.Online,
                 FieldRequirementType = FieldRequirementType.JustOne,
                 Fields = new List<Field>{
                     new ()
                     {
                         Label = "Abone No",
                         Length = 10,
                         Mask = "##########",
                         Order = 1,
                         Pattern = "##########",
                         Placeholder = "1234567890",
                         Prefix = "1",
                         Suffix = "1"
                     }
                 }
            }
        };
    }

    public async Task<BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse>> GetInstitutionPaymentDetailsAsync(ReconcilliationInstitutionDetailRequest reconciliationDetailsRequest)
    {
        return new BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse>
        {
            IsSuccess = true,
            Response = new ReconciliationInstitutionPaymentDetailsSummaryResponse
            {
                InstitutionId = reconciliationDetailsRequest.InstitutionId,
                PaymentAmount = 30,
                PaymentCount = 1,
                CancellationAmount = 30,
                CancellationCount = 1,
                PaymentTransactions = new List<PaymentTransaction>()
            }
        };
    }

    public async Task<BillingResponse<ReconciliationDetailsResponse>> GetReconciliationDetailsAsync(ReconciliationDetailsRequest reconcilationDetailsRequest)
    {
        var response = new BillingResponse<ReconciliationDetailsResponse> { IsSuccess = true };

        try
        {
            if (reconcilationDetailsRequest.InstitutionId != Guid.Empty)
            {
                await _vendorMapper.GetVendorInstitutionByInstitutionIdAsync(reconcilationDetailsRequest.InstitutionId, _vendor.Id);

                response.Response = new ReconciliationDetailsResponse
                {
                    ReconciliationDate = ParseDate(DateTime.Now.ToString("yyyyMMdd")),
                    ReconciliationDetails = new List<InstitutionReconciliation>
                        {
                            new InstitutionReconciliation
                            {
                                CancellationAmount =  0,
                                CancellationCount =  0,
                                PaymentAmount =  0,
                                PaymentCount =  0,
                                InstitutionId = reconcilationDetailsRequest.InstitutionId,
                                ReconciliationStatus = ReconciliationStatus.Fail
                            }
                        }
                };
            }
            else
            {
                await _vendorMapper.GetInstitutionMappingsByVendorAsync(_vendor.Id);

                response.Response = new ReconciliationDetailsResponse
                {
                    ReconciliationDate = ParseDate(DateTime.Now.ToString("yyyyMMdd")),
                    ReconciliationDetails = new List<InstitutionReconciliation> { new InstitutionReconciliation
                    {
                        CancellationAmount =  0,
                        CancellationCount = 0,
                        PaymentAmount = 0,
                        PaymentCount = 0,
                        InstitutionId = Guid.Empty,
                        ReconciliationStatus = ReconciliationStatus.Fail
                    }}
                };

            }
        }
        catch (ReconciliationException reconciliationException)
        {
            response.ErrorMessage = reconciliationException.Message;
            response.ErrorCode = reconciliationException.Code;
        }

        return response;
    }

    public async Task<BillingResponse<ReconciliationSummaryResponse>> GetReconciliationSummaryAsync(TransactionStatistics statistics)
    {
        return new BillingResponse<ReconciliationSummaryResponse>
        {
            IsSuccess = true,
            Response = new ReconciliationSummaryResponse
            {
                CancellationAmount = 30,
                PaymentAmount = 30,
                CancellationCount = 1,
                PaymentCount = 1,
                ReconciliationDate = ParseDate(DateTime.Now.ToString("yyyyMMdd")),
                ReconciliationStatus = ReconciliationStatus.Success
            }
        };
    }

    public async Task<List<SectorMapping>> GetSectorListAsync()
    {
        return new List<SectorMapping>
        {
            new()
            {
                VendorId =_vendor.Id,
                VendorSectorId = "MockSector",
            }
        };
    }

    public async Task<BillingResponse<BillInquiryResponse>> InquireBillsAsync(InquireBillQuery billInquiryRequest)
    {
        var response = new BillingResponse<BillInquiryResponse>
        {
            IsSuccess = true,
            Response = new BillInquiryResponse
            {
                RequestId = _randomGenerator.GenerateSecureRandomNumber(1, 10000000000).ToString(),
                InstitutionId = billInquiryRequest.InstitutionId,
                VendorId = _vendor.Id,
                Bills = new List<Bill> {
                    new()
                    {
                        Amount = 30,
                        Currency = "TRY",
                        Date = DateTime.Now,
                        DueDate = DateTime.Now.AddDays(1),
                        Number = "00000001",
                        Id = "3d00000000D000L1",
                        SubscriberName = "MockBank",
                        SubscriberNumber1 = "00000001",
                        SubscriberNumber2 = "00000002",
                        SubscriberNumber3 = "00000003"
                    }
                }
            }
        };

        return response;
    }

    public async Task<BillingResponse<BillStatusResponse>> InquireBillStatusAsync(BillStatusRequest billStatusRequest)
    {
        var response = new BillingResponse<BillStatusResponse>
        {
            IsSuccess = true,
            Response = new BillStatusResponse
            {
                InstitutionId = billStatusRequest.InstitutionId,
                DebtOid = _randomGenerator.GenerateSecureRandomNumber(1, 10000000000).ToString(),
                BpcOid = _randomGenerator.GenerateSecureRandomNumber(1, 10000000000).ToString(),
                BillStatus = BillStatus.Paid
            }
        };

        return response;
    }

    public async Task<BillingResponse<InstitutionReconciliationCloseResponse>> InstitutionReconciliationCloseAsync(InstitutionReconciliationCloseRequest institutionReconciliationCloseRequest)
    {
        return new BillingResponse<InstitutionReconciliationCloseResponse>
        {
            IsSuccess = true,
            Response = new InstitutionReconciliationCloseResponse
            {
                ReconciliationDate = ParseDate(DateTime.Now.ToString("yyyyMMdd")),
                InstitutionReconciliations = new List<InstitutionReconciliationCloseDetails>
                {
                    new()
                    {
                        ReconciliationDate = ParseDate(institutionReconciliationCloseRequest.ReconciliationDate.ToString()),
                        ReconciliationStatus = ReconciliationStatus.Success,
                        InstitutionId = _vendorMapper.GetInstitutionMappingsByVendorAsync(_vendor.Id).Result.Select(im=>im.InstitutionId).FirstOrDefault(),
                        PaymentAmount = 30,
                        PaymentCount = 1,
                        CancellationAmount = 30,
                        CancellationCount = 1,
                    }
                }
            }
        };
    }

    public async Task<BillingResponse<BillPaymentResponse>> PayInquiredBillsAsync(PayInquiredBillCommand billPaymentRequest)
    {
        var isoCurrencyCode = GetIsoCurrencyCode("TL");

        var convertedDateTime = ParseDate(DateTime.Now.ToString("yyyyMMdd"));

        var response = new BillingResponse<BillPaymentResponse>
        {
            IsSuccess = true,
            Response = new BillPaymentResponse
            {
                InstitutionId = billPaymentRequest.InstitutionId,
                RequestId = billPaymentRequest.RequestId,
                VendorId = _vendor.Id,
                Invoice = new BillInvoice
                {
                    Id = Random.Shared.Next().ToString(),
                    Amount = 30,
                    Number = "00000001",
                    Currency = isoCurrencyCode,
                    ReferenceId = "0000000001",
                    VoucherNumber = "0000001",
                    Description = "Cancelled",
                    IsSuccess = true,
                    SubscriberNumber1 = "00000001",
                    DueDate = convertedDateTime,
                    Date = convertedDateTime
                }
            }
        };

        return response;
    }

    public Task ValidateRequestAsync(Guid institutionId, string subscriberNumber1, string subscriberNumber2, string subscriberNumber3)
    {
        return Task.CompletedTask;
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
