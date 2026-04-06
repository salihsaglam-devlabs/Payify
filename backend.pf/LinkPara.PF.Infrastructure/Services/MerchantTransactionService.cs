using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantTransactions;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BTrans;
using LinkPara.HttpProviders.BTrans.Models;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Location;
using LinkPara.HttpProviders.Location.Models;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.HttpProviders.MoneyTransfer.Models;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions.Command.PatchMerchantTransaction;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.Pagination;
using MassTransit;
using TransactionSource = LinkPara.SharedModels.Banking.Enums.TransactionSource;
using TransactionStatus = LinkPara.PF.Domain.Enums.TransactionStatus;

namespace LinkPara.PF.Infrastructure.Services;

public class MerchantTransactionService : IMerchantTransactionService
{
    private readonly PfDbContext _pfDbContext;
    private readonly IMapper _mapper;
    private readonly ILogger<MerchantTransactionService> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IContextProvider _contextProvider;
    private readonly IParameterService _parameterService;
    private readonly ICacheService _cacheService;
    private readonly ISourceBankAccountService _sourceBankAccountService;
    private readonly ILocationService _locationService;
    private readonly IBTransPosInformationService _bTransPosInformationService;

    private const string PaymentWiredToMainMerchantDefaultDescription = "Ödemelerin tamamı ana bayiye yapılmaktadır";

    public MerchantTransactionService(PfDbContext pfDbContext,
        IMapper mapper,
        ILogger<MerchantTransactionService> logger,
        IAuditLogService auditLogService,
        IApplicationUserService applicationUserService,
        IParameterService parameterService,
        IContextProvider contextProvider,ICacheService cacheService,
        ISourceBankAccountService sourceBankAccountService,
        ILocationService locationService,
        IBTransPosInformationService bTransPosInformationService)
    {
        _pfDbContext = pfDbContext;
        _mapper = mapper;
        _logger = logger;
        _auditLogService = auditLogService;
        _applicationUserService = applicationUserService;
        _contextProvider = contextProvider;
        _cacheService = cacheService;
        _sourceBankAccountService = sourceBankAccountService;
        _locationService = locationService;
        _bTransPosInformationService = bTransPosInformationService;
        _parameterService = parameterService;
    }

    public async Task<UpdateMerchantTransactionRequest> PatchAsync(
        PatchMerchantTransactionCommand patchMerchantTransactionCommand)
    {
        var merchantTransaction = await _pfDbContext
            .MerchantTransaction
            .FirstOrDefaultAsync(b => b.Id == patchMerchantTransactionCommand.Id);


        if (merchantTransaction is null)
        {
            throw new NotFoundException(nameof(MerchantTransaction), patchMerchantTransactionCommand.Id);
        }

        try
        {
            var strategy = _pfDbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                _pfDbContext.Attach(merchantTransaction);

                var merchantTransactionMap = _mapper.Map<UpdateMerchantTransactionRequest>(merchantTransaction);

                patchMerchantTransactionCommand.MerchantTransaction.ApplyTo(merchantTransactionMap);

                if (merchantTransaction.IsSuspecious == merchantTransactionMap.IsSuspecious &&
                    merchantTransaction.IsChargeback == merchantTransactionMap.IsChargeback)
                {
                    return merchantTransactionMap;
                }

                var deductionType = ValidateAndReturnDeduction(merchantTransaction, merchantTransactionMap);

                _mapper.Map(merchantTransactionMap, merchantTransaction);

                var postingTransactions = await _pfDbContext
                    .PostingTransaction
                    .Where(w =>
                        w.MerchantTransactionId.Equals(merchantTransaction.Id)
                        && w.RecordStatus == RecordStatus.Active
                    ).ToListAsync();

                if (postingTransactions.Count > 0)
                {
                    await ManageDeductionAsync(merchantTransaction, deductionType);
                    await ManageBTransAsync(merchantTransaction, postingTransactions, deductionType);
                    _pfDbContext.PostingTransaction.UpdateRange(postingTransactions);
                }

                merchantTransaction.LastChargebackActivityDate = DateTime.Now;

                await _pfDbContext.SaveChangesAsync();

                scope.Complete();

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "UpdateMerchantTransaction",
                        SourceApplication = "PF",
                        Resource = "AcquireBank",
                        Details = new Dictionary<string, string>
                        {
                            { "Id", merchantTransaction.Id.ToString() },
                            { "IsChargeback", merchantTransactionMap.IsChargeback.ToString() },
                            { "IsSuspecious", merchantTransactionMap.IsSuspecious.ToString() },
                            { "SuspeciousDescription", merchantTransactionMap.SuspeciousDescription }
                        }
                    });

                return merchantTransactionMap;
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantTransactionPatchError : {exception}");
            throw;
        }
    }

    public async Task<List<MerchantTransactionDto>> GetMerchantTransactionsAsync(MerchantTransactionRequest request)
    {
        var merchantTransactions = await _pfDbContext.MerchantTransaction
            .Where(s => s.MerchantId == request.MerchantId
                        && s.RecordStatus == RecordStatus.Active
                        && s.CreateDate >= request.StartDate
                        && s.CreateDate <= request.EndDate
                        && s.TransactionType == request.TransactionType)
            .ToListAsync();

        return _mapper.Map<List<MerchantTransactionDto>>(merchantTransactions);
    }

    private async Task ManageDeductionAsync(
        MerchantTransaction merchantTransaction,
        DeductionType deductionType)
    {
        var merchant = await _pfDbContext.Merchant.FirstOrDefaultAsync(s => s.Id == merchantTransaction.MerchantId);

        var createdBy = _contextProvider.CurrentContext.UserId ?? _applicationUserService.ApplicationUserId.ToString();

        if (deductionType is DeductionType.Chargeback or DeductionType.Suspicious)
        {
            var deductionAmountType =
                await _parameterService.GetParameterAsync("PFPaymentParameters", "WaiveCommissionOnChargeback");

            var isPaymentMadeToParentMerchant =
                merchantTransaction.PfCommissionRate + merchantTransaction.ParentMerchantCommissionRate == 100;

            var deductionAmount = deductionAmountType.ParameterValue == "1"
                ? merchantTransaction.AmountWithoutCommissions
                : merchantTransaction.Amount - merchantTransaction.ParentMerchantCommissionAmount;

            var merchantDeduction = new MerchantDeduction
            {
                TotalDeductionAmount = !isPaymentMadeToParentMerchant ? deductionAmount : 0,
                RemainingDeductionAmount = !isPaymentMadeToParentMerchant ? deductionAmount : 0,
                Currency = merchantTransaction.Currency,
                DeductionType = deductionType,
                DeductionStatus = isPaymentMadeToParentMerchant ? DeductionStatus.Completed : DeductionStatus.Pending,
                ExecutionDate = DateTime.Now,
                MerchantId = merchantTransaction.MerchantId,
                MerchantTransactionId = merchantTransaction.Id,
                MerchantDueId = Guid.Empty,
                RecordStatus = RecordStatus.Active,
                CreatedBy = createdBy,
                PostingBalanceId = Guid.Empty,
                ConversationId = merchantTransaction.ConversationId,
                DeductionAmountWithCommission = merchantTransaction.Amount,
                SubMerchantDeductionId = Guid.Empty
            };

            await _pfDbContext.MerchantDeduction.AddAsync(merchantDeduction);

            if (merchant.MerchantType == MerchantType.SubMerchant &&
                merchant.ParentMerchantId.HasValue &&
                merchant.ParentMerchantId.Value != Guid.Empty &&
                merchantTransaction.ParentMerchantCommissionAmount > 0)
            {
                var parentMerchantDeductionType = deductionType switch
                {
                    DeductionType.Chargeback => DeductionType.ChargebackCommission,
                    DeductionType.Suspicious => DeductionType.SuspiciousCommission,
                    _ => throw new ArgumentException($"UnsupportedDeductionType: {deductionType}")
                };

                decimal parentMerchantDeductionAmount;
                if (isPaymentMadeToParentMerchant)
                {
                    parentMerchantDeductionAmount =
                        deductionAmountType.ParameterValue == "1"
                            ? merchantTransaction.ParentMerchantCommissionAmount
                            : merchantTransaction.ParentMerchantCommissionAmount +
                              merchantTransaction.PfCommissionAmount;
                }
                else
                {
                    parentMerchantDeductionAmount = merchantTransaction.ParentMerchantCommissionAmount;
                }

                var parentMerchantDeduction = new MerchantDeduction
                {
                    TotalDeductionAmount = parentMerchantDeductionAmount,
                    RemainingDeductionAmount = parentMerchantDeductionAmount,
                    Currency = merchantTransaction.Currency,
                    DeductionType = parentMerchantDeductionType,
                    DeductionStatus = DeductionStatus.Pending,
                    ExecutionDate = DateTime.Now,
                    MerchantId = merchant.ParentMerchantId.Value,
                    MerchantTransactionId = merchantTransaction.Id,
                    MerchantDueId = Guid.Empty,
                    RecordStatus = RecordStatus.Active,
                    CreatedBy = createdBy,
                    PostingBalanceId = Guid.Empty,
                    ConversationId = merchantTransaction.ConversationId,
                    DeductionAmountWithCommission = merchantTransaction.ParentMerchantCommissionAmount +
                                                    merchantTransaction.PfCommissionAmount,
                    SubMerchantDeductionId = merchantDeduction.Id
                };

                await _pfDbContext.MerchantDeduction.AddAsync(parentMerchantDeduction);
            }
        }
        else
        {
            //Rejected Chargeback or Suspicious
            var typeToCheck = deductionType switch
            {
                DeductionType.RejectedChargeback => DeductionType.Chargeback,
                DeductionType.RejectedSuspicious => DeductionType.Suspicious,
                _ => throw new ArgumentException($"UnsupportedDeductionType: {deductionType}")
            };

            var parentTypeToCheck = deductionType switch
            {
                DeductionType.RejectedChargeback => DeductionType.ChargebackCommission,
                DeductionType.RejectedSuspicious => DeductionType.SuspiciousCommission,
                _ => throw new ArgumentException($"UnsupportedDeductionType: {deductionType}")
            };

            var lastCreatedDeduction = await _pfDbContext
                .MerchantDeduction
                .Where(w =>
                    w.MerchantId == merchantTransaction.MerchantId &&
                    w.MerchantTransactionId == merchantTransaction.Id &&
                    w.DeductionType == typeToCheck &&
                    w.RecordStatus == RecordStatus.Active
                )
                .OrderByDescending(s => s.CreateDate)
                .FirstOrDefaultAsync();

            if (lastCreatedDeduction is null)
            {
                return;
            }

            if (lastCreatedDeduction.DeductionStatus == DeductionStatus.Processing)
            {
                throw new MerchantTransactionUnavailableException();
            }

            if (merchant.MerchantType == MerchantType.SubMerchant &&
                merchant.ParentMerchantId.HasValue &&
                merchant.ParentMerchantId.Value != Guid.Empty)
            {
                var lastCreatedCommissionDeduction = await _pfDbContext
                    .MerchantDeduction
                    .Where(w =>
                        w.MerchantId == merchant.ParentMerchantId.Value &&
                        w.MerchantTransactionId == merchantTransaction.Id &&
                        w.DeductionType == parentTypeToCheck &&
                        w.SubMerchantDeductionId == lastCreatedDeduction.Id &&
                        w.RecordStatus == RecordStatus.Active
                    )
                    .OrderByDescending(s => s.CreateDate)
                    .FirstOrDefaultAsync();

                if (lastCreatedCommissionDeduction is not null)
                {
                    if (lastCreatedCommissionDeduction.DeductionStatus == DeductionStatus.Processing)
                    {
                        throw new MerchantTransactionUnavailableException();
                    }
                    //ParentMerchant Commission deduction exists
                    _pfDbContext.MerchantDeduction.Attach(lastCreatedCommissionDeduction);

                    if (lastCreatedCommissionDeduction.TotalDeductionAmount ==
                        lastCreatedCommissionDeduction.RemainingDeductionAmount)
                    {
                        //No related deduction occured yet
                        lastCreatedCommissionDeduction.DeductionStatus = DeductionStatus.Resolved;
                    }
                    else
                    {
                        //Related deduction occurred

                        var parentRejectedType = deductionType switch
                        {
                            DeductionType.RejectedChargeback => DeductionType.RejectedChargebackCommission,
                            DeductionType.RejectedSuspicious => DeductionType.RejectedSuspiciousCommission,
                            _ => throw new ArgumentException($"UnsupportedDeductionType: {deductionType}")
                        };

                        var deductedAmount =
                            lastCreatedCommissionDeduction.TotalDeductionAmount -
                            lastCreatedCommissionDeduction.RemainingDeductionAmount;

                        var parentCommissionRejectedDeduction = new MerchantDeduction
                        {
                            TotalDeductionAmount = deductedAmount,
                            RemainingDeductionAmount = deductedAmount,
                            Currency = lastCreatedCommissionDeduction.Currency,
                            DeductionType = parentRejectedType,
                            DeductionStatus = DeductionStatus.Pending,
                            ExecutionDate = DateTime.Now,
                            MerchantId = lastCreatedCommissionDeduction.MerchantId,
                            MerchantTransactionId = lastCreatedCommissionDeduction.MerchantTransactionId,
                            MerchantDueId = Guid.Empty,
                            RecordStatus = RecordStatus.Active,
                            CreatedBy = createdBy,
                            PostingBalanceId = Guid.Empty,
                            ConversationId = lastCreatedCommissionDeduction.ConversationId,
                            DeductionAmountWithCommission = deductedAmount,
                            SubMerchantDeductionId = Guid.Empty
                        };
                        await _pfDbContext.MerchantDeduction.AddAsync(parentCommissionRejectedDeduction);
                        lastCreatedCommissionDeduction.DeductionStatus = DeductionStatus.PartialDeductionResolve;
                    }
                }
            }

            //original deduction return operations
            _pfDbContext.MerchantDeduction.Attach(lastCreatedDeduction);

            if (lastCreatedDeduction.DeductionStatus is DeductionStatus.Pending or DeductionStatus.Completed)
            {
                if (lastCreatedDeduction.TotalDeductionAmount ==
                    lastCreatedDeduction.RemainingDeductionAmount)
                {
                    //No related deduction occured yet
                    lastCreatedDeduction.DeductionStatus = DeductionStatus.Resolved;
                }
                else
                {
                    //Related deduction occurred

                    var deductedAmount =
                        lastCreatedDeduction.TotalDeductionAmount - lastCreatedDeduction.RemainingDeductionAmount;

                    var rejectedDeduction = new MerchantDeduction
                    {
                        TotalDeductionAmount = deductedAmount,
                        RemainingDeductionAmount = deductedAmount,
                        Currency = lastCreatedDeduction.Currency,
                        DeductionType = deductionType,
                        DeductionStatus = DeductionStatus.Pending,
                        ExecutionDate = DateTime.Now,
                        MerchantId = lastCreatedDeduction.MerchantId,
                        MerchantTransactionId = lastCreatedDeduction.MerchantTransactionId,
                        MerchantDueId = Guid.Empty,
                        RecordStatus = RecordStatus.Active,
                        CreatedBy = createdBy,
                        PostingBalanceId = Guid.Empty,
                        ConversationId = lastCreatedDeduction.ConversationId,
                        DeductionAmountWithCommission = deductedAmount,
                        SubMerchantDeductionId = Guid.Empty
                    };
                    await _pfDbContext.MerchantDeduction.AddAsync(rejectedDeduction);
                    lastCreatedDeduction.DeductionStatus = DeductionStatus.PartialDeductionResolve;
                }
            }
            else
            {
                //Transferred or PartialTransfer

                var transferTypeToCheck = lastCreatedDeduction.DeductionType switch
                {
                    DeductionType.Chargeback => DeductionType.ChargebackTransfer,
                    DeductionType.Suspicious => DeductionType.SuspiciousTransfer,
                    _ => throw new ArgumentException($"UnsupportedDeductionType: {lastCreatedDeduction.DeductionType}")
                };

                var transferredParentDeduction = await _pfDbContext
                    .MerchantDeduction
                    .Where(w =>
                        w.MerchantId == merchant.ParentMerchantId.Value &&
                        w.DeductionType == transferTypeToCheck &&
                        w.SubMerchantDeductionId == lastCreatedDeduction.Id &&
                        w.RecordStatus == RecordStatus.Active
                    )
                    .OrderByDescending(s => s.CreateDate)
                    .FirstOrDefaultAsync();

                _pfDbContext.MerchantDeduction.Attach(transferredParentDeduction);

                //transferredParentDeduction handling
                if (transferredParentDeduction.TotalDeductionAmount ==
                    transferredParentDeduction.RemainingDeductionAmount)
                {
                    //No related deduction occurred on transferred deduction yet
                    transferredParentDeduction.DeductionStatus = DeductionStatus.Resolved;
                }
                else
                {
                    //Related deduction occurred on transferred deduction
                    var parentRejectedType = transferredParentDeduction.DeductionType switch
                    {
                        DeductionType.ChargebackTransfer => DeductionType.RejectedChargebackTransfer,
                        DeductionType.SuspiciousTransfer => DeductionType.RejectedSuspiciousTransfer,
                        _ => throw new ArgumentException($"UnsupportedDeductionType: {deductionType}")
                    };

                    var deductedAmount =
                        transferredParentDeduction.TotalDeductionAmount -
                        transferredParentDeduction.RemainingDeductionAmount;

                    var parentTransferredRejectedDeduction = new MerchantDeduction
                    {
                        TotalDeductionAmount = deductedAmount,
                        RemainingDeductionAmount = deductedAmount,
                        Currency = transferredParentDeduction.Currency,
                        DeductionType = parentRejectedType,
                        DeductionStatus = DeductionStatus.Pending,
                        ExecutionDate = DateTime.Now,
                        MerchantId = transferredParentDeduction.MerchantId,
                        MerchantTransactionId = transferredParentDeduction.MerchantTransactionId,
                        MerchantDueId = Guid.Empty,
                        RecordStatus = RecordStatus.Active,
                        CreatedBy = createdBy,
                        PostingBalanceId = Guid.Empty,
                        ConversationId = transferredParentDeduction.ConversationId,
                        DeductionAmountWithCommission = deductedAmount,
                        SubMerchantDeductionId = Guid.Empty
                    };
                    await _pfDbContext.MerchantDeduction.AddAsync(parentTransferredRejectedDeduction);
                    transferredParentDeduction.DeductionStatus = DeductionStatus.PartialDeductionResolve;
                }

                //Submerchant deduction handling
                if (lastCreatedDeduction.DeductionStatus is DeductionStatus.Transferred)
                {
                    lastCreatedDeduction.DeductionStatus = DeductionStatus.TransferResolve;
                }
                else
                {
                    lastCreatedDeduction.DeductionStatus = DeductionStatus.PartialTransferResolve;

                    var deductedAmount =
                        lastCreatedDeduction.TotalDeductionAmount - lastCreatedDeduction.RemainingDeductionAmount;

                    var rejectedDeduction = new MerchantDeduction
                    {
                        TotalDeductionAmount = deductedAmount,
                        RemainingDeductionAmount = deductedAmount,
                        Currency = lastCreatedDeduction.Currency,
                        DeductionType = deductionType,
                        DeductionStatus = DeductionStatus.Pending,
                        ExecutionDate = DateTime.Now,
                        MerchantId = lastCreatedDeduction.MerchantId,
                        MerchantTransactionId = lastCreatedDeduction.MerchantTransactionId,
                        MerchantDueId = Guid.Empty,
                        RecordStatus = RecordStatus.Active,
                        CreatedBy = createdBy,
                        PostingBalanceId = Guid.Empty,
                        ConversationId = lastCreatedDeduction.ConversationId,
                        DeductionAmountWithCommission = deductedAmount,
                        SubMerchantDeductionId = Guid.Empty
                    };
                    await _pfDbContext.MerchantDeduction.AddAsync(rejectedDeduction);
                }
            }
        }
    }

    private static DeductionType ValidateAndReturnDeduction(MerchantTransaction merchantTransaction,
        UpdateMerchantTransactionRequest request)
    {
        if (
            merchantTransaction.TransactionType is not (TransactionType.Auth or TransactionType.PostAuth) ||
            merchantTransaction.TransactionStatus is not TransactionStatus.Success)
        {
            throw new TransactionTypeNotValidForDeductionException();
        }

        if ((request.IsChargeback && request.IsSuspecious) ||
            (merchantTransaction.IsChargeback && request.IsSuspecious) ||
            (merchantTransaction.IsSuspecious && request.IsChargeback))
        {
            throw new TransactionCanBeChargebackOrSuspiciousException();
        }

        if (merchantTransaction.IsChargeback && !request.IsChargeback)
        {
            return DeductionType.RejectedChargeback;
        }
        else if (!merchantTransaction.IsChargeback && request.IsChargeback)
        {
            return DeductionType.Chargeback;
        }
        else if (merchantTransaction.IsSuspecious && !request.IsSuspecious)
        {
            return DeductionType.RejectedSuspicious;
        }
        else if (!merchantTransaction.IsSuspecious && request.IsSuspecious)
        {
            return DeductionType.Suspicious;
        }
        else
        {
            throw new InvalidOperationException($"UnknownDeductionTypeMerchantTransactionId: {merchantTransaction.Id}" +
                                                $"ChargebackValues: {merchantTransaction.IsChargeback} -> {request.IsChargeback}" +
                                                $"SuspiciousValues: {merchantTransaction.IsSuspecious} -> {request.IsSuspecious}");
        }
    }

    private async Task ManageBTransAsync(MerchantTransaction merchantTransaction, List<PostingTransaction> postingTransactions,
        DeductionType deductionType)
    {
        var postingBalances = await
            _pfDbContext.PostingBalance
                .Where(b => postingTransactions.Select(s => s.PostingBalanceId).ToList().Contains(b.Id))
                .Include(t => t.Merchant.Customer.AuthorizedPerson)
                .Include(t => t.Merchant.MerchantBankAccounts)
                .ThenInclude(t => t.Bank)
                .ToListAsync();
        
        var sourceBankAccounts = await _cacheService.GetOrCreateAsync("BTransSourceBankAccounts",
            async () => await _sourceBankAccountService.GetAllSourceBankAccountsAsync(
                new GetSourceBankAccountsRequest
                {
                    Source = TransactionSource.PF,
                    AccountType = BankAccountType.UsageAccount,
                    RecordStatus = RecordStatus.Active
                }));
        
        var currency = await _pfDbContext.Currency
            .Where(s => s.Number == merchantTransaction.Currency)
            .FirstOrDefaultAsync();
        
        var headMerchant = await _pfDbContext.Merchant
            .Where(s => s.Id == merchantTransaction.MerchantId)
            .Include(s => s.Customer)
            .ThenInclude(s => s.AuthorizedPerson)
            .Include(s => s.MerchantBankAccounts)
            .ThenInclude(s => s.Bank)
            .FirstOrDefaultAsync();
        
        var vpos = await _pfDbContext.Vpos
            .Where(s => s.Id == merchantTransaction.VposId)
            .Include(s => s.AcquireBank.Bank)
            .Include(b => b.VposBankApiInfos
                .Where(a => a.RecordStatus == RecordStatus.Active))
            .ThenInclude(s => s.Key)
            .FirstOrDefaultAsync();
        
        var physicalPos = await _pfDbContext.MerchantPhysicalPos
            .Where(s => s.Id == merchantTransaction.MerchantPhysicalPosId)
            .Include(s => s.PhysicalPos.AcquireBank.Bank)
            .FirstOrDefaultAsync();

        var location = await _cacheService.GetOrCreateAsync(
            $"BTransCityWithCountryDto:{postingBalances.First().Merchant.Customer.City}",
            async () => await _locationService.GetCountryByCityCode(postingBalances.First().Merchant.Customer.City));
        var headMerchantLocation = headMerchant?.Customer?.City is { } city
            ? await _locationService.GetCountryByCityCode(city)
            : location;

        foreach (var postingTransaction in postingTransactions)
        {
            if (deductionType is DeductionType.Chargeback or DeductionType.Suspicious)
            {
                if (postingTransaction.BTransStatus != PostingBTransStatus.Completed)
                {
                    var posReport = await PopulateBTransNewRecordAsync(
                        postingTransaction, 
                        postingBalances, 
                        currency, 
                        location,
                        sourceBankAccounts, 
                        headMerchant,
                        headMerchantLocation, 
                        vpos, 
                        physicalPos);
                    await _bTransPosInformationService.CreatePosInformationRecordsAsync(
                        new CreatePosInformationRecordsRequest { PosInformationReports = posReport });
                }

                await _bTransPosInformationService.DeletePosInformationRecordAsync(
                    new DeletePosInformationRecordRequest
                        { RelatedTransactionId = merchantTransaction.Id, RecordType = RecordTypeConst.DeleteRecord }
                );
            }
            else
            {
                var posReport = await PopulateBTransNewRecordAsync(
                    postingTransaction, 
                    postingBalances, 
                    currency, 
                    location,
                    sourceBankAccounts, 
                    headMerchant,
                    headMerchantLocation, 
                    vpos, 
                    physicalPos);
                await _bTransPosInformationService.CreatePosInformationRecordsAsync(
                    new CreatePosInformationRecordsRequest { PosInformationReports = posReport });
            }

            postingTransaction.BTransStatus = PostingBTransStatus.Completed;
        }
    }

    private async Task<PosInformationReportList> PopulateBTransNewRecordAsync(
        PostingTransaction postingTransaction, 
        List<PostingBalance> postingBalances,
        Currency currency, 
        CityWithCountryDto location,
        PaginatedList<SourceBankAccountDto> sourceBankAccounts, 
        Merchant headMerchant, 
        CityWithCountryDto headMerchantLocation,
        Vpos vpos, 
        MerchantPhysicalPos physicalPos)
    {
        var postingBalance = postingBalances.First(s => s.Id == postingTransaction.PostingBalanceId);
        var sourceBank = sourceBankAccounts.Items
            .FirstOrDefault(a =>
                a.BankCode == postingBalance.MoneyTransferBankCode && a.CurrencyCode == currency?.Code);

        var bankAccount =
            postingBalance.Merchant.MerchantBankAccounts.FirstOrDefault(t => t.Iban == postingBalance.Iban) ??
            postingBalance.Merchant.MerchantBankAccounts.FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);

        var isReceiverSubCompany = postingBalance.Merchant.MerchantType == MerchantType.SubMerchant;
        var headCompanyTaxNumber = headMerchant?.Customer?.TaxNumber ?? "";
        var headCompanyCommercialTitle = headMerchant?.Customer?.CommercialTitle ?? "";

        var organizationDescription = postingTransaction.OrderId;
        var receiverIban = bankAccount?.Iban;
        var receiverBankName = bankAccount?.Bank.Name;
        var receiverBankCode = bankAccount?.BankCode.ToString();
        var receiverWalletNumber = postingBalance.WalletNumber;
        var relatedBalanceId = postingBalance.Id;

        var receiverTaxNumber = postingBalance.Merchant.Customer.TaxNumber;
        var receiverCommercialTitle = postingBalance.Merchant.Customer.CommercialTitle;
        var receiverIdentityNumber = postingBalance.Merchant.Customer.AuthorizedPerson.IdentityNumber;
        var receiverFirstName = postingBalance.Merchant.Customer.AuthorizedPerson.Name;
        var receiverLastName = postingBalance.Merchant.Customer.AuthorizedPerson.Surname;
        var receiverNationCountryId = location.Country.Iso2;
        var receiverFullAddress = postingBalance.Merchant.Customer.Address;
        var receiverDistrict = postingBalance.Merchant.Customer.DistrictName;
        var receiverPostalCode = postingBalance.Merchant.Customer.PostalCode;
        var receiverCityId = location.City.Iso2;
        var receiverCity = postingBalance.Merchant.Customer.CityName;
        var receiverPhoneNumber = postingBalance.Merchant.Customer.AuthorizedPerson.MobilePhoneNumber;

        var totalAmount = postingTransaction.Amount;
        var netAmount = postingTransaction.AmountWithoutCommissions;
        var commissionAmount = postingTransaction.PfCommissionAmount;
        var parentCommissionAmount = postingTransaction.ParentMerchantCommissionAmount;
        
        var posBankName = ""; 
        var posBankCode = ""; 
        var posMerchantId = "";
        var posTerminalId = "";
        if (vpos is not null)
        {
            posBankName = vpos.AcquireBank.Bank.Name; 
            posBankCode = vpos.AcquireBank.Bank.Code.ToString(); 
            posMerchantId = vpos.VposBankApiInfos
                .FirstOrDefault(i => i.Key.Category == BankApiKeyCategory.MerchantId)?.Value;
            posTerminalId = vpos.VposBankApiInfos
                .FirstOrDefault(i => i.Key.Category == BankApiKeyCategory.TerminalId)?.Value;
        }else if (physicalPos is not null)
        {
            posBankName = physicalPos.PhysicalPos.AcquireBank.Bank.Name; 
            posBankCode = physicalPos.PhysicalPos.AcquireBank.Bank.Code.ToString(); 
            posMerchantId = physicalPos.PosMerchantId;
            posTerminalId = physicalPos.PosTerminalId;
        }
       
        if (postingTransaction.PfCommissionRate + postingTransaction.ParentMerchantCommissionRate == 100)
        {
            string paymentWiredToMainMerchantDescription;
            try
            {
                paymentWiredToMainMerchantDescription =
                    (await _parameterService.GetParametersAsync("PostingParams"))
                    .FirstOrDefault(w => w.ParameterCode == "PaymentWiredToMainMerchantBTransDescription")
                    ?.ParameterValue;
            }
            catch
            {
                paymentWiredToMainMerchantDescription = PaymentWiredToMainMerchantDefaultDescription;
            }

            organizationDescription =
                (paymentWiredToMainMerchantDescription + "-" + postingTransaction.OrderId).Truncate(300);

            var headMerchantCommissionBalanceId = await _pfDbContext.PostingAdditionalTransaction
                .Where(s =>
                    s.RelatedPostingBalanceId == postingBalance.Id &&
                    s.TransactionType == TransactionType.ParentMerchantCommission)
                .Select(s => s.PostingBalanceId)
                .FirstOrDefaultAsync();

            var headMerchantCommissionBalance = await _pfDbContext.PostingBalance
                .Where(s => s.Id == headMerchantCommissionBalanceId)
                .FirstOrDefaultAsync();

            var headMerchantBankAccount =
                headMerchant?.MerchantBankAccounts?.FirstOrDefault(t =>
                    t.Iban == headMerchantCommissionBalance?.Iban) ??
                headMerchant?.MerchantBankAccounts?.FirstOrDefault(s => s.RecordStatus == RecordStatus.Active);

            receiverIban = headMerchantBankAccount?.Iban;
            receiverBankName = headMerchantBankAccount?.Bank.Name;
            receiverBankCode = headMerchantBankAccount?.BankCode.ToString();
            receiverWalletNumber = headMerchantCommissionBalance?.WalletNumber;
            relatedBalanceId = headMerchantCommissionBalance?.Id ?? postingBalance.Id;

            receiverTaxNumber = headMerchant?.Customer?.TaxNumber;
            receiverCommercialTitle = headMerchant?.Customer?.CommercialTitle;
            receiverIdentityNumber = headMerchant?.Customer?.AuthorizedPerson.IdentityNumber;
            receiverFirstName = headMerchant?.Customer?.AuthorizedPerson?.Name;
            receiverLastName = headMerchant?.Customer?.AuthorizedPerson?.Surname;
            receiverNationCountryId = headMerchantLocation.Country.Iso2;
            receiverFullAddress = headMerchant?.Customer?.Address;
            receiverDistrict = headMerchant?.Customer?.DistrictName;
            receiverPostalCode = headMerchant?.Customer?.PostalCode;
            receiverCityId = headMerchantLocation.City.Iso2;
            receiverCity = headMerchant?.Customer?.CityName;
            receiverPhoneNumber = headMerchant?.Customer?.AuthorizedPerson?.MobilePhoneNumber;

            isReceiverSubCompany = false;
            headCompanyTaxNumber = "";
            headCompanyCommercialTitle = "";

            netAmount = parentCommissionAmount;
            parentCommissionAmount = 0;
        }

        var posInformationReportList = new PosInformationReportList
        {
            PosInformationReports =
            [
                new PosInformationReport
                {
                    OperationType = postingTransaction.PfTransactionSource == PfTransactionSource.VirtualPos ? PosInformationConst.VirtualPos : PosInformationConst.PhysicalPos,
                    RecordType = RecordTypeConst.NewRecord,

                    PosBankName = posBankName,
                    PosBankCode = posBankCode,
                    PosMerchantId = posMerchantId,
                    PosTerminalId = posTerminalId,

                    SenderBankName = postingBalance.MoneyTransferBankName,
                    SenderBankCode = postingBalance.MoneyTransferBankCode.ToString(),
                    SenderAccountNumber = sourceBank?.AccountNumber,
                    SenderIbanNumber = sourceBank?.IBANNumber,

                    IsReceiverCustomer = true,
                    IsReceiverCorporate = true,
                    ReceiverTaxNumber = receiverTaxNumber,
                    ReceiverCommercialTitle = receiverCommercialTitle,
                    ReceiverIdentityNumber = receiverIdentityNumber,
                    ReceiverFirstName = receiverFirstName,
                    ReceiverLastName = receiverLastName,
                    ReceiverNationCountryId = receiverNationCountryId,
                    ReceiverFullAddress = receiverFullAddress,
                    ReceiverDistrict = receiverDistrict,
                    ReceiverPostalCode = receiverPostalCode,
                    ReceiverCityId = receiverCityId,
                    ReceiverCity = receiverCity,
                    ReceiverPhoneNumber = receiverPhoneNumber,
                    ReceiverAccountNumber = receiverIban,
                    ReceiverBankName = receiverBankName,
                    ReceiverBankCode = receiverBankCode,
                    ReceiverIbanNumber = receiverIban,
                    ReceiverWalletNumber = receiverWalletNumber,

                    CurrencyCode = currency?.Code,
                    RelatedTransactionId = postingTransaction.MerchantTransactionId,
                    TransactionDate = postingTransaction.TransactionDate,
                    PaymentDate = postingTransaction.PaymentDate,
                    NetAmount = netAmount,
                    ConvertedAmount = netAmount,
                    Amount = totalAmount,
                    TotalPricingAmount = commissionAmount,
                    OrganizationDescription = organizationDescription,

                    IsReceiverSubCompany = isReceiverSubCompany,
                    HeadCompanyTaxNumber = headCompanyTaxNumber,
                    HeadCompanyCommercialTitle = headCompanyCommercialTitle,
                    HeadCompanyPricingAmount = parentCommissionAmount,

                    RelatedBalanceId = relatedBalanceId,
                    RelatedInstallmentTransactionId = postingTransaction.MerchantInstallmentTransactionId
                }
            ]
        };

        return posInformationReportList;
    }
}