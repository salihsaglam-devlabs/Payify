using System.Globalization;
using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Commons.Models.Billing;
using LinkPara.Billing.Application.Commons.Models.BillingModels.Enums;
using LinkPara.Billing.Application.Commons.Models.Reconciliation;
using LinkPara.Billing.Application.Features.Billing.Commands.PayInquiredBill;
using LinkPara.Billing.Application.Features.InstitutionApis.Queries.GetBillInquiry;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.Billing.Infrastructure.CorporationPaymentsService;
using LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Interfaces;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Billing.Infrastructure.Services.BillingServices;

public class VakifKatilimBillingService : IBillingVendorService
{
    private readonly IVakifKatilimApi _vakifKatilimApi;
    private readonly IStringLocalizer _localizer;
    private readonly Vendor _vendor;
    private readonly IGenericRepository<Sector> _sectorRepository;
    private readonly IGenericRepository<TransactionReferenceCounter> _transactionReferenceCounterRepository;
    private readonly IAccountService _emoneyService;
    private IGenericRepository<InstitutionMapping> _institutionMappingRepository;

    public VakifKatilimBillingService(
        IVakifKatilimApi vakifKatilimApi,
        IVendorService vendorService,
        IStringLocalizerFactory factory, 
        IGenericRepository<Sector> sectorRepository, 
        IGenericRepository<TransactionReferenceCounter> transactionReferenceCounterRepository,
        IAccountService emoneyService, 
        IGenericRepository<InstitutionMapping> institutionMappingRepository )
    {
        _vakifKatilimApi = vakifKatilimApi;
        _sectorRepository = sectorRepository;
        _transactionReferenceCounterRepository = transactionReferenceCounterRepository;
        _emoneyService = emoneyService;
        _institutionMappingRepository = institutionMappingRepository;
        _vendor = vendorService.GetByNameAsync("VakifKatilim").Result;
        _localizer = factory.Create("ScreenFields", "LinkPara.Billing.API");
    }
    public async Task<List<SectorMapping>> GetSectorListAsync()
    {
        var response = await _vakifKatilimApi.GetInstitutionListAsync();
        
        var sectors = response
            .Select(x => new SectorMapping
                {
                    Id = _vendor.Id,
                    VendorSectorId = GetSectorName(x.InstitutionType)
                })
            .DistinctBy(s => s.VendorSectorId)
            .ToList();
        
        return sectors;
    }

    public async Task<List<InstitutionMapping>> GetInstitutionListAsync()
    {
        var res = await _vakifKatilimApi.GetInstitutionListAsync();
        
        var institutionList = res.Select(x => new InstitutionMapping
        {
            Institution = new Institution
            {
                RecordStatus = RecordStatus.Active,
                Name = x.InstitutionLongName,
                Sector = new Sector
                {
                    Name = GetSectorName(x.InstitutionType)
                },
                OperationMode = x.IsOnline ? OperationMode.Online : OperationMode.Offline,
                FieldRequirementType = FieldRequirementType.Unknown,
                Fields = new List<Field>()
                {
                    new()
                    {
                        RecordStatus = RecordStatus.Active,
                        Label = "AboneNo",
                        Mask = "##########",
                        Pattern = "##########",
                        Placeholder = "1234567890",
                        Length = 10,
                        Order = 1,
                        Prefix = string.Empty,
                        Suffix = string.Empty
                    }
                }
            },
            VendorInstitutionId = x.InstitutionId.ToString(),
            VendorId = _vendor.Id,
            Vendor = _vendor,
            RecordStatus = RecordStatus.Active
        }).ToList();

        return institutionList;
    }

    public async Task<List<Institution>> GetInstitutionListBySectorAsync(Guid sectorId)
    {
        var res = await _vakifKatilimApi.GetInstitutionListAsync();
        
        var institutions =  res.Select(x => new Institution
        {
            RecordStatus = RecordStatus.Active,
            Name = x.InstitutionLongName,
            Sector = new Sector
            {
                Name = GetSectorName(x.InstitutionType)
            },
            OperationMode = x.IsOnline ? OperationMode.Online : OperationMode.Offline,
            FieldRequirementType = FieldRequirementType.JustOne,
            Fields = null
        })
            .ToList();
        
        var sector = await _sectorRepository.GetByIdAsync(sectorId);

        if (sector == null)
        {
            return new List<Institution>();
        }
        
        return institutions
            .Where(x => x.Sector.Name == sector.Name)
            .ToList();
    }

    public async Task<BillingResponse<BillInquiryResponse>> InquireBillsAsync(InquireBillQuery billInquiryRequest)  
    {
        var institutionMapping = await _institutionMappingRepository.GetAll()
            .FirstOrDefaultAsync(i => i.InstitutionId == billInquiryRequest.InstitutionId 
                                      && i.VendorId == _vendor.Id
                                      && i.RecordStatus == RecordStatus.Active);
               
        if (institutionMapping == null)
        {
            throw new NotFoundException(nameof(InstitutionMapping), billInquiryRequest.InstitutionId);
        }
        var res = await _vakifKatilimApi.InquireBillsAsync(int.Parse(institutionMapping.VendorInstitutionId), billInquiryRequest.SubscriberNumber1, _vendor);
        
        var referenceGuid = Guid.NewGuid();
        
        return new BillingResponse<BillInquiryResponse>
        {
            IsSuccess = res.Success,
            ErrorCode = res.Results.Length > 0 ? res.Results[0]?.ErrorCode : null,
            ErrorMessage = res.Results.Length > 0 ?res.Results[0]?.ErrorMessage : null,
            Response = res.Success ? new BillInquiryResponse
            {
                Bills = res.BillList.Select(x => new Bill
                {
                    Id = x.DebtId?.ToString(),
                    Number = x.BillNumber,
                    Date = x.InvoiceDate ?? DateTime.MinValue,
                    Amount = x.Amount.HasValue ? // if bill has delay amount, add it to the amount
                        (x.InvoiceDelayAmount.HasValue ? 
                            x.Amount.Value + x.InvoiceDelayAmount.Value 
                            : x.Amount.Value) 
                        : 0,
                    CommissionAmount = 0,
                    Currency = "TRY",
                    SubscriberName = x.NameSurname,
                    SubscriberNumber1 = x.SubscriberNumber,
                    SubscriberNumber2 = null,
                    SubscriberNumber3 = null,
                    DueDate = x.LastPaymenDate ?? DateTime.MinValue
                }).ToList(),
                RequestId = referenceGuid.ToString(),
                VendorId = _vendor.Id,
                InstitutionId = billInquiryRequest.InstitutionId
            } : null
        };
    }

    public async Task<BillingResponse<BillPaymentResponse>> PayInquiredBillsAsync(PayInquiredBillCommand billPaymentRequest)
    {
        var transactionReference = await _transactionReferenceCounterRepository
            .GetAll()
            .Where(x => x.TransactionReferenceGuid == Guid.Parse(billPaymentRequest.RequestId))
            .FirstOrDefaultAsync();
        
        if (transactionReference == null)
        {
            transactionReference = await _transactionReferenceCounterRepository.AddAsync(new TransactionReferenceCounter
            {
                TransactionReferenceGuid = Guid.Parse(billPaymentRequest.RequestId),
               
            });
        }
        
        var institutionMapping = await _institutionMappingRepository.GetAll()
            .FirstOrDefaultAsync(i => i.InstitutionId == billPaymentRequest.InstitutionId 
                                      && i.VendorId == _vendor.Id
                                      && i.RecordStatus == RecordStatus.Active);
               
        if (institutionMapping == null)
        {
            throw new NotFoundException(nameof(InstitutionMapping), billPaymentRequest.InstitutionId);
        }
        
        var billInquiry = await _vakifKatilimApi.InquireBillsAsync(int.Parse(institutionMapping.VendorInstitutionId), billPaymentRequest.Bill.SubscriberNumber1, _vendor);

        var bill = billInquiry.BillList.FirstOrDefault(s => s.BillNumber == billPaymentRequest.Bill.Number);
        
        if(bill != null)
        {
            CalculateBillAmount(bill);
        }

        var account = await _emoneyService.GetAccountDetailAsync(new GetAccountDetailRequest
        {
            WalletNumber = billPaymentRequest.WalletNumber
        });
        
        var res = await _vakifKatilimApi.DoPaymentAsync(transactionReference.TransactionReferenceInt, bill, billPaymentRequest, account.IdentityNumber);
        
        return new BillingResponse<BillPaymentResponse>
        {
            IsSuccess = res.Success,
            ErrorCode = res.Success ? string.Empty : res.Results[0].ErrorCode,
            ErrorMessage = res.Success ? string.Empty :res.Results[0].ErrorMessage,
            Response = res.Success ? new BillPaymentResponse
            {
                RequestId = billPaymentRequest.RequestId,
                InstitutionId = billPaymentRequest.InstitutionId,
                VendorId = _vendor.Id,
                Invoice = new BillInvoice
                {
                    Id = bill!.DebtId?.ToString(),
                    Number = bill.BillNumber,
                    Date = bill.InvoiceDate ?? DateTime.MinValue,
                    Amount = bill.Amount ?? 0,
                    CommissionAmount = 0,
                    Currency = "TRY",
                    SubscriberName = bill.NameSurname,
                    SubscriberNumber1 = bill.SubscriberNumber,
                    SubscriberNumber2 = null,
                    SubscriberNumber3 = null,
                    IsSuccess = true,
                    DueDate = bill.LastPaymenDate ?? DateTime.MinValue
                }
            } : null
        };
    }

    private void CalculateBillAmount(BillList bill)
    {
        bill.DebtAmount = bill.Amount;
        
        bill.Amount = bill.Amount.HasValue
            ? // if bill has delay amount, add it to the amount
            (bill.InvoiceDelayAmount.HasValue
                ? bill.Amount.Value + bill.InvoiceDelayAmount.Value
                : bill.Amount.Value)
            : 0;
    }

    public async Task<BillingResponse<BillCancelResponse>> CancelBillPaymentAsync(BillCancelRequest billCancelRequest)
    {
       var res = await _vakifKatilimApi.CancelPaymentAsync(int.Parse(billCancelRequest.RequestId));

       return new BillingResponse<BillCancelResponse>
       {
           IsSuccess = res.Success,
           ErrorCode = res.Success ? string.Empty : res.Results[0].ErrorCode,
           ErrorMessage = res.Success ? string.Empty : res.Results[0].ErrorMessage,
           Response =
           {
               RequestId = billCancelRequest.RequestId,
               InstitutionId = billCancelRequest.InstitutionId,
               VendorId = _vendor.Id
           }
       };
    }

    public async Task<BillingResponse<BillStatusResponse>> InquireBillStatusAsync(BillStatusRequest billStatusRequest)
    {
        var transactionReference = await _transactionReferenceCounterRepository.GetByIdAsync(billStatusRequest.RequestId);
        
        if(transactionReference == null)
        {
            throw new NotFoundException(nameof(TransactionReferenceCounter), billStatusRequest.RequestId);
        }
        
        var res = await _vakifKatilimApi.ControlPaymentAsync(int.Parse(billStatusRequest.RequestId));
        
        return new BillingResponse<BillStatusResponse>
        {
            IsSuccess = res.Success,
            ErrorCode = res.Success ? string.Empty : res.Results[0].ErrorCode,
            ErrorMessage = res.Success ? string.Empty : res.Results[0].ErrorMessage,
            Response = res.Success ? new BillStatusResponse
            {
                InstitutionId = billStatusRequest.InstitutionId,
                BpcOid = null,
                DebtOid = res.TransactionReference.ToString(CultureInfo.InvariantCulture),
                BillStatus = res.PaymentStatus switch
                {
                    0 => BillStatus.NotFound,
                    1 => BillStatus.AccountingFinished,
                    2 => BillStatus.Notified,
                    3 => BillStatus.PaymentCancelled,
                    4 => BillStatus.ReconciliationCancelled,
                    _ => throw new InvalidBillStatusException(_vendor, $"InvalidBillStatusValue: {res.PaymentStatus}")
                }
            } : null
        };
    }

    public async Task<BillingResponse<ReconciliationSummaryResponse>> GetReconciliationSummaryAsync(TransactionStatistics statistics)
    {
        var res = await _vakifKatilimApi.ReconciliationSummaryAsync(statistics.StatisticsDate);
        
        return new BillingResponse<ReconciliationSummaryResponse>
        {
            IsSuccess = res.Success,
            ErrorCode = res.Success ? string.Empty : res.Results[0].ErrorCode,
            ErrorMessage = res.Success ? string.Empty : res.Results[0].ErrorMessage,
            Response = res.Success ? new ReconciliationSummaryResponse
            {
                PaymentAmount = res.TotalPaymentAmount,
                CancellationAmount = res.TotalCancelAmount,
                PaymentCount = res.TotalPaymentCount,
                CancellationCount = res.TotalCancelCount,
                ReconciliationDate = res.ReconciliationDate,
                ReconciliationStatus = ReconciliationStatus.Success
            } : null
        };
    }

    public async Task<BillingResponse<ReconciliationDetailsResponse>> GetReconciliationDetailsAsync(ReconciliationDetailsRequest reconcilationDetailsRequest)
    {
        var institutionMapping = await _institutionMappingRepository.GetAll()
            .FirstOrDefaultAsync(i => i.InstitutionId == reconcilationDetailsRequest.InstitutionId 
                                      && i.VendorId == _vendor.Id
                                      && i.RecordStatus == RecordStatus.Active);
        
       var res = await _vakifKatilimApi.ReconciliationSummaryByInstitutionAsync(reconcilationDetailsRequest.ReconciliationDate, int.Parse(institutionMapping.VendorInstitutionId));
       
        return new BillingResponse<ReconciliationDetailsResponse>
        {
            IsSuccess = res.Success,
            ErrorCode = res.Success ? string.Empty : res.Results[0].ErrorCode,
            ErrorMessage = res.Success ? string.Empty : res.Results[0].ErrorMessage,
            Response = res.Success ? new ReconciliationDetailsResponse
            {
                ReconciliationDate = reconcilationDetailsRequest.ReconciliationDate,
                ReconciliationDetails = res.InsReconciliationSummary.Select(x => new InstitutionReconciliation
                {
                    PaymentAmount = x.InsTotalPaymentAmount,
                    CancellationAmount = x.InsTotalCancelAmount,
                    PaymentCount = x.InsTotalPaymentCount,
                    CancellationCount = x.InsTotalCancelCount,
                    InstitutionSummaryId = institutionMapping.InstitutionId,
                    InstitutionId = institutionMapping.InstitutionId
                }).ToList()
            } : null
        };
    }

    public async Task<BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse>> GetInstitutionPaymentDetailsAsync(ReconcilliationInstitutionDetailRequest reconciliationDetailsRequest)
    {
        var institutionMapping = await _institutionMappingRepository.GetAll()
            .FirstOrDefaultAsync(i => i.InstitutionId == reconciliationDetailsRequest.InstitutionId 
                                      && i.VendorId == _vendor.Id
                                      && i.RecordStatus == RecordStatus.Active);
        
        var res = await _vakifKatilimApi.ReconciliationSummaryByInstitutionAsync(reconciliationDetailsRequest.ReconciliationDate, int.Parse(institutionMapping.VendorInstitutionId));
        
        var detailRes = await _vakifKatilimApi.ReconcilationSummaryDetailAsync(reconciliationDetailsRequest.ReconciliationDate, int.Parse(institutionMapping.VendorInstitutionId), false);
        
        return new BillingResponse<ReconciliationInstitutionPaymentDetailsSummaryResponse>
        {
            IsSuccess = res.Success,
            ErrorCode = res.Success ? string.Empty : res.Results[0].ErrorCode,
            ErrorMessage = res.Success ? string.Empty : res.Results[0].ErrorMessage,
            Response = res.Success ? new ReconciliationInstitutionPaymentDetailsSummaryResponse
            {
                InstitutionId = reconciliationDetailsRequest.InstitutionId,
                PaymentTransactions = detailRes.TransactionsPaymentEntity.Select(x => new PaymentTransaction
                {
                    BillDate = x.TransactionDate,
                    BillNumber = x.BillNo,
                    BillDueDate = x.TransactionDate,
                    BillAmount = x.PaymentAmount,
                    BillAmountCurrency = "TRY",
                    PaymentAmount = x.PaymentAmount,
                    PaymentAmountCurrency = "TRY",
                    PaymentReferenceId = x.TransactionId.ToString(),
                    SubscriberNumber1 = x.SubscriberId,
                    SubscriberNumber2 = null,
                    SubscriberNumber3 = null,
                    TransactionStatus = x.IsCancelled ? TransactionStatus.Cancelled : TransactionStatus.Paid
                }).ToList(),
                PaymentAmount = res.TotalPaymentAmount,
                CancellationAmount = res.TotalCancelAmount,
                PaymentCount = res.TotalPaymentCount,
                CancellationCount = res.TotalCancelCount,
            } : null
        };
    }

    public Task<BillingResponse<InstitutionReconciliationCloseResponse>> InstitutionReconciliationCloseAsync(InstitutionReconciliationCloseRequest institutionReconciliationCloseRequest)
    {
        throw new NotImplementedException();
    }

    public Task ValidateRequestAsync(Guid institutionId, string subscriberNumber1, string subscriberNumber2,
        string subscriberNumber3)
    {
       // do nothing
        return Task.CompletedTask;
    }
    
    private string GetSectorName(int institutionType)
    {
        var originalCulture = Thread.CurrentThread.CurrentUICulture;
        try
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("tr-TR");

            var type = institutionType switch
            {
                1 => _localizer["Gas"],
                2 => _localizer["Water"],
                3 => _localizer["Internet"],
                4 => _localizer["Electricity"],
                6 => _localizer["HouseFee"],
                10 => _localizer["Rent"],
                _ => _localizer["Other"]
            };

            return type;
        }
        finally
        {
            Thread.CurrentThread.CurrentUICulture = originalCulture;
        }
    }
}