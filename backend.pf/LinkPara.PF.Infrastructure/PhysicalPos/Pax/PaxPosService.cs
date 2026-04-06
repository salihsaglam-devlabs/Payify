using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Application.Commons.Models.PhysicalPos;
using LinkPara.PF.Application.Commons.Models.PhysicalPos.Constants;
using LinkPara.PF.Application.Commons.Models.PhysicalPos.Response;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxEndOfDayCommand;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxParameterCommand;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxReconciliationCommand;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxTransactionCommand;
using LinkPara.PF.Application.Features.Transactions;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.PF.Infrastructure.Services.PaymentServices;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;
using TransactionStatus = LinkPara.PF.Domain.Enums.TransactionStatus;

namespace LinkPara.PF.Infrastructure.PhysicalPos.Pax;

public class PaxPosService : IPaxPosService
{
    private readonly ILogger<PaxPosService> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IOrderNumberGeneratorService _orderNumberGeneratorService;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IContextProvider _contextProvider;
    private readonly IPosRouterService _posRouterService;
    private readonly IBus _bus;
    private readonly IBasePaymentService _basePaymentService;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<MerchantInstallmentTransaction> _merchantInstallmentTransactionRepository;

    private const string GenericErrorCode = "99";
    private const string GenericSuccessCode = "00";

    public PaxPosService(
        ILogger<PaxPosService> logger,
        PfDbContext dbContext,
        IResponseCodeService errorCodeService,
        IContextProvider contextProvider,
        IPosRouterService posRouterService,
        IOrderNumberGeneratorService orderNumberGeneratorService,
        IBus bus,
        IBasePaymentService basePaymentService,
        IParameterService parameterService,
        IGenericRepository<MerchantInstallmentTransaction> merchantInstallmentTransactionRepository)
    {
        _logger = logger;
        _dbContext = dbContext;
        _errorCodeService = errorCodeService;
        _contextProvider = contextProvider;
        _posRouterService = posRouterService;
        _orderNumberGeneratorService = orderNumberGeneratorService;
        _bus = bus;
        _basePaymentService = basePaymentService;
        _parameterService = parameterService;
        _merchantInstallmentTransactionRepository = merchantInstallmentTransactionRepository;
    }

    public async Task<PaxTransactionResponse> TransactionAsync(PaxTransactionCommand request)
    {
        try
        {
            if (request.Gateway != nameof(Gateway.PFPosGateway))
            {
                return await UnacceptableAsync(request, ApiErrorCode.InvalidGateway);
            }

            var merchant = await _dbContext.Merchant.Where(s => s.Id == request.PfMerchantId).FirstOrDefaultAsync();
            if (merchant is null)
            {
                return await UnacceptableAsync(request, ApiErrorCode.InvalidMerchant);
            }

            if (merchant.MerchantStatus != MerchantStatus.Active)
            {
                return await UnacceptableAsync(request, ApiErrorCode.InvalidMerchantStatus);
            }

            var activeMerchantTransaction = await _dbContext.MerchantTransaction
                .FirstOrDefaultAsync(b => b.MerchantId == request.PfMerchantId
                                          && b.OrderId == request.PaymentId
                                          && b.RecordStatus == RecordStatus.Active);
            if (activeMerchantTransaction is not null)
            {
                return await UnacceptableAsync(request, ApiErrorCode.DuplicateMerchantTransaction);
            }

            var device = await _dbContext.DeviceInventory.Where(s => s.SerialNo == request.SerialNumber)
                .FirstOrDefaultAsync();
            if (device is null)
            {
                return await UnacceptableAsync(request, ApiErrorCode.PhysicalDeviceNotFound);
            }

            var merchantPhysicalDevice =
                await _dbContext.MerchantPhysicalDevice
                    .Where(s => s.DeviceInventoryId == device.Id && s.MerchantId == request.PfMerchantId)
                    .FirstOrDefaultAsync();
            if (merchantPhysicalDevice is null || merchantPhysicalDevice.RecordStatus != RecordStatus.Active)
            {
                return await UnacceptableAsync(request, ApiErrorCode.MerchantPhysicalDeviceNotFound);
            }

            var merchantPhysicalPos = await _dbContext.MerchantPhysicalPos
                .Include(s => s.PhysicalPos)
                .ThenInclude(a => a.AcquireBank)
                .Include(s => s.PhysicalPos.Currencies)
                .Where(s =>
                    s.MerchantPhysicalDeviceId == merchantPhysicalDevice.Id &&
                    s.PosMerchantId == request.MerchantId &&
                    s.PosTerminalId == request.TerminalId &&
                    s.RecordStatus == RecordStatus.Active &&
                    s.PhysicalPos.Currencies.Any(a => a.CurrencyCode == request.Currency))
                .FirstOrDefaultAsync();
            if (merchantPhysicalPos is null)
            {
                return await UnacceptableAsync(request, ApiErrorCode.MerchantPhysicalPosNotFound);
            }

            request.ClientIpAddress ??= _contextProvider.CurrentContext.ClientIpAddress;
            request.Installment = request.Installment == 1 ? 0 : request.Installment;

            var transactionDate = TransactionDateFromUnixTimestamp(request.Date);

            return request.Type switch
            {
                PaxTransactionType.Sale or PaxTransactionType.InstallmentSale => await AuthAsync(request,
                    merchantPhysicalPos, merchant, transactionDate),
                PaxTransactionType.Refund => await ReturnAsync(request, merchantPhysicalPos, merchant, transactionDate),
                PaxTransactionType.Void => await ReverseAsync(request, merchantPhysicalPos, merchant, transactionDate),
                _ => await UnacceptableAsync(request, ApiErrorCode.InvalidTransactionType)
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Pax Transaction Error: {exception}");
            return new PaxTransactionResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId
            };
        }
    }

    private async Task<MerchantTransaction> PopulateInitialMerchantTransactionAsync(
        PaxTransactionCommand request,
        MerchantPhysicalPos merchantPhysicalPos,
        Merchant merchant,
        PhysicalPosEndOfDay eod,
        PhysicalPosRouteResponse routeInfo,
        DateTime transactionDate,
        string conversationId)
    {
        return new MerchantTransaction
        {
            MerchantId = merchant.Id,
            ConversationId = conversationId ??
                             await _orderNumberGeneratorService.GenerateForPhysicalPosTransactionAsync(
                                 routeInfo.AcquireBank.BankCode, merchant.Number, merchant.Name),
            IpAddress = request.ClientIpAddress,
            TransactionType = request.Type switch
            {
                PaxTransactionType.Sale or PaxTransactionType.InstallmentSale => TransactionType.Auth,
                PaxTransactionType.Void => TransactionType.Reverse,
                PaxTransactionType.Refund => TransactionType.Return,
                _ => TransactionType.Auth
            },
            TransactionStatus = request.Status switch
            {
                PaxTransactionStatus.Approved => TransactionStatus.Success,
                _ => TransactionStatus.Fail
            },
            OrderId = request.PaymentId,
            Amount = routeInfo.Amount,
            PointAmount = routeInfo.PointAmount,
            PointCommissionRate = routeInfo.PointCommissionRate,
            PointCommissionAmount = routeInfo.PointCommissionAmount,
            ServiceCommissionRate = routeInfo.ServiceCommissionRate,
            ServiceCommissionAmount = routeInfo.ServiceCommissionAmount,
            Currency = routeInfo.CurrencyNumber,
            InstallmentCount = request.Installment,
            BinNumber = request.BinNumber,
            CardNumber = request.MaskedCardNo,
            HasCvv = false,
            HasExpiryDate = false,
            IsInternational = routeInfo.IsInternational,
            IsAmex = routeInfo.IsAmex,
            IsReverse = false,
            IsManualReturn = false,
            IsOnUsPayment = false,
            IsInsurancePayment = false,
            IsReturn = false,
            ReturnAmount = 0,
            IsPreClose = false,
            Is3ds = false,
            BankCommissionRate = routeInfo.BankCommissionRate,
            BankCommissionAmount = routeInfo.BankCommissionAmount,
            IssuerBankCode = routeInfo.IssuerBankCode,
            AcquireBankCode = routeInfo.AcquireBank.BankCode,
            CardTransactionType = routeInfo.CardTransactionType,
            IntegrationMode = IntegrationMode.Api,
            ResponseCode = GenericSuccessCode,
            ResponseDescription = GenericSuccessCode,
            TransactionStartDate = transactionDate,
            TransactionEndDate = transactionDate,
            VposId = Guid.Empty,
            LanguageCode = "TR",
            BatchStatus = BatchStatus.EodPending,
            CardType = routeInfo.CardType,
            TransactionDate = transactionDate.Date,
            IsChargeback = false,
            IsTopUpPayment = false,
            IsSuspecious = false,
            LastChargebackActivityDate = DateTime.MinValue,
            Description = $"InstitutionId:{request.InstitutionId}, " +
                          $"PosEntryMode:{PaxPosEntryMode.GetName(request.PosEntryMode)}, " +
                          $"PinEntryMode:{PaxPinEntryInfo.GetName(request.PinEntryInfo)}",
            ReturnStatus = ReturnStatus.NoAction,
            CreatedNameBy = request.Vendor,
            PfCommissionAmount = routeInfo.PfCommissionAmount,
            PfNetCommissionAmount = routeInfo.PfNetCommissionAmount,
            PfCommissionRate = routeInfo.PfCommissionRate,
            PfPerTransactionFee = routeInfo.PfPerTransactionFee,
            ParentMerchantCommissionAmount = routeInfo.ParentMerchantCommissionAmount,
            ParentMerchantCommissionRate = routeInfo.ParentMerchantCommissionRate,
            AmountWithoutCommissions = routeInfo.AmountWithoutCommissions,
            AmountWithoutBankCommission = routeInfo.AmountWithoutBankCommission,
            AmountWithoutParentMerchantCommission = routeInfo.AmountWithoutParentMerchantCommission,
            PricingProfileItemId = routeInfo.PricingProfileItem.Id,
            BsmvAmount = routeInfo.BsmvAmount,
            ProvisionNumber = request.ProvisionNo,
            PfPaymentDate = routeInfo.PfPaymentDate,
            BankPaymentDate = routeInfo.BankPaymentDate,
            PostingItemId = Guid.Empty,
            BlockageStatus = BlockageStatus.None,
            PfTransactionSource = PfTransactionSource.PhysicalPos,
            MerchantPhysicalPosId = merchantPhysicalPos.Id,
            PhysicalPosEodId = eod.Id,
            EndOfDayStatus = EndOfDayStatus.Pending,
            IsPerInstallment = routeInfo.ProfileSettlementMode == ProfileSettlementMode.PerInstallment ? true : false
        };
    }

    private async Task<MerchantInstallmentTransaction> PopulateInitialMerchantInstallmentTransaction(
       MerchantTransaction merchantTransaction, int installmentNumber, decimal amount, int pricingBlockedDayNumber,
    int costBlockedDayNumber, PricingProfile pricingProfile,
    PricingProfileItem pricingProfileItem, MerchantInstallmentTransaction referenceMerchantInstallmentTransaction, string vendor, decimal remainingReturnAmount = 0)
    {
        var merchantInstallmentTransaction = new MerchantInstallmentTransaction
        {
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            CreatedBy = merchantTransaction.CreatedBy,
            CreatedNameBy = merchantTransaction.CreatedNameBy,
            IpAddress = merchantTransaction.IpAddress,
            TransactionStartDate = merchantTransaction.TransactionStartDate,
            TransactionDate = merchantTransaction.TransactionDate,
            TransactionType = merchantTransaction.TransactionType,
            ConversationId = merchantTransaction.ConversationId,
            MerchantId = merchantTransaction.MerchantId,
            IntegrationMode = merchantTransaction.IntegrationMode,
            IsPreClose = merchantTransaction.IsPreClose,
            IsReverse = merchantTransaction.IsReverse,
            IsReturn = merchantTransaction.IsReturn,
            IsManualReturn = merchantTransaction.IsManualReturn,
            IsOnUsPayment = merchantTransaction.IsOnUsPayment,
            IsInsurancePayment = merchantTransaction.IsInsurancePayment,
            ReturnAmount = merchantTransaction.ReturnAmount,
            BankCommissionRate = merchantTransaction.BankCommissionRate,
            Currency = merchantTransaction.Currency,
            Amount = amount,
            PointAmount = merchantTransaction.PointAmount,
            InstallmentCount = installmentNumber,
            ThreeDSessionId = merchantTransaction.ThreeDSessionId,
            Is3ds = merchantTransaction.Is3ds,
            CardNumber = merchantTransaction.CardNumber,
            BinNumber = merchantTransaction.BinNumber,
            HasCvv = merchantTransaction.HasCvv,
            HasExpiryDate = merchantTransaction.HasExpiryDate,
            IsAmex = merchantTransaction.IsAmex,
            IsInternational = merchantTransaction.IsInternational,
            VposId = merchantTransaction.VposId,
            IssuerBankCode = merchantTransaction.IssuerBankCode,
            AcquireBankCode = merchantTransaction.AcquireBankCode,
            CardTransactionType = merchantTransaction.CardTransactionType,
            CardHolderName = merchantTransaction.CardHolderName,
            MerchantCustomerName = merchantTransaction.MerchantCustomerName,
            MerchantCustomerPhoneNumber = merchantTransaction.MerchantCustomerPhoneNumber,
            MerchantCustomerPhoneCode = merchantTransaction.MerchantCustomerPhoneCode,
            LanguageCode = merchantTransaction.LanguageCode,
            BatchStatus = merchantTransaction.BatchStatus,
            OrderId = merchantTransaction.OrderId,
            PostingItemId = merchantTransaction.PostingItemId,
            BlockageStatus = merchantTransaction.BlockageStatus,
            LastChargebackActivityDate = merchantTransaction.LastChargebackActivityDate,
            IsTopUpPayment = merchantTransaction.IsTopUpPayment,
            PfTransactionSource = merchantTransaction.PfTransactionSource,
            CardHolderIdentityNumber = merchantTransaction.CardHolderIdentityNumber,
            EndOfDayStatus = merchantTransaction.EndOfDayStatus,
            SubMerchantId = merchantTransaction.SubMerchantId,
            SubMerchantName = merchantTransaction.SubMerchantName,
            SubMerchantNumber = merchantTransaction.SubMerchantNumber,
            CardType = merchantTransaction.CardType,
            Description = merchantTransaction.Description,
            IsChargeback = merchantTransaction.IsChargeback,
            IsSuspecious = merchantTransaction.IsSuspecious,
            MerchantPhysicalPosId = merchantTransaction.MerchantPhysicalPosId,
            VposName = merchantTransaction.VposName,
            MerchantTransactionId = merchantTransaction.Id,
            PricingProfileItemId = pricingProfileItem.Id,
            PreCloseDate = merchantTransaction.PreCloseDate,
            PreCloseTransactionId = merchantTransaction.PreCloseTransactionId,
            ReturnDate = merchantTransaction.ReturnDate,
            ReturnedTransactionId = merchantTransaction.ReturnedTransactionId,
            ReturnStatus = merchantTransaction.ReturnStatus,
            ReverseDate = merchantTransaction.ReverseDate,
            SuspeciousDescription = merchantTransaction.SuspeciousDescription,
            TransactionEndDate = DateTime.Now,
            ResponseCode = GenericSuccessCode,
            ResponseDescription = GenericSuccessCode,
            TransactionStatus = TransactionStatus.Success,
            ProvisionNumber = merchantTransaction.ProvisionNumber,
            PhysicalPosEodId = merchantTransaction.PhysicalPosEodId,
            PhysicalPosOldEodId = merchantTransaction.PhysicalPosOldEodId,
            AmountWithoutBankCommission = 0,
            AmountWithoutCommissions = 0,
            AmountWithoutParentMerchantCommission = 0,
            BankCommissionAmount = 0,
            BsmvAmount = 0,
            ParentMerchantCommissionAmount = 0,
            ParentMerchantCommissionRate = 0,
            PfCommissionAmount = 0,
            PfCommissionRate = 0,
            PfNetCommissionAmount = 0,
            PfPerTransactionFee = 0,
            PointCommissionAmount = 0,
            PointCommissionRate = 0,
            ServiceCommissionAmount = 0,
            ServiceCommissionRate = 0,

        };

        merchantInstallmentTransaction.Amount = amount;

        if (merchantTransaction.TransactionType == TransactionType.Return)
        {
            merchantInstallmentTransaction.ReturnedTransactionId = referenceMerchantInstallmentTransaction.Id.ToString();

            merchantInstallmentTransaction.BankCommissionAmount = (merchantTransaction.BankCommissionRate / 100m) * amount;

            var perTransactionFee = remainingReturnAmount == 0 ? pricingProfileItem.PricingProfile.PerTransactionFee : 0;

            merchantInstallmentTransaction.PfCommissionAmount = perTransactionFee
                + pricingProfileItem.CommissionRate / 100m * amount;

            merchantInstallmentTransaction.PfNetCommissionAmount = merchantInstallmentTransaction.PfCommissionAmount
                - merchantInstallmentTransaction.BankCommissionAmount;

            merchantInstallmentTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;
            merchantInstallmentTransaction.PfPerTransactionFee = perTransactionFee;

            merchantInstallmentTransaction.ParentMerchantCommissionAmount =
                pricingProfileItem.ParentMerchantCommissionRate / 100m * amount;

            merchantInstallmentTransaction.ParentMerchantCommissionRate =
                pricingProfileItem.ParentMerchantCommissionRate;

            merchantInstallmentTransaction.AmountWithoutCommissions = amount
                - merchantInstallmentTransaction.PfCommissionAmount
                - merchantInstallmentTransaction.ParentMerchantCommissionAmount;

            merchantInstallmentTransaction.AmountWithoutBankCommission = amount
                - merchantInstallmentTransaction.BankCommissionAmount;

            merchantInstallmentTransaction.AmountWithoutParentMerchantCommission = amount
                - merchantInstallmentTransaction.ParentMerchantCommissionAmount;


            merchantInstallmentTransaction.BsmvAmount = await BsmvAmountCalculateHelper
                .CalculateBsmvAmount(merchantInstallmentTransaction.PfCommissionAmount - merchantInstallmentTransaction.BankCommissionAmount, _parameterService);

            merchantInstallmentTransaction.PfPaymentDate = merchantTransaction.PfPaymentDate;

            merchantInstallmentTransaction.PointCommissionRate = merchantTransaction.PointCommissionRate;

            merchantInstallmentTransaction.PointCommissionAmount = merchantTransaction.PointCommissionAmount;

            merchantInstallmentTransaction.ServiceCommissionRate = merchantTransaction.ServiceCommissionRate;

            merchantInstallmentTransaction.ServiceCommissionAmount =
                (amount * merchantTransaction.ServiceCommissionRate) / 100m;

            merchantInstallmentTransaction.BankPaymentDate = merchantTransaction.BankPaymentDate;

            if (referenceMerchantInstallmentTransaction != null)
            {
                var transactionStatus = amount == referenceMerchantInstallmentTransaction.Amount
                ? TransactionStatus.Returned
                : TransactionStatus.PartiallyReturned;

                referenceMerchantInstallmentTransaction.ReturnAmount = amount;
                referenceMerchantInstallmentTransaction.ReturnDate = merchantTransaction.TransactionStartDate;
                referenceMerchantInstallmentTransaction.IsReturn = true;
                referenceMerchantInstallmentTransaction.TransactionStatus = transactionStatus;
                referenceMerchantInstallmentTransaction.LastModifiedBy = vendor;
                referenceMerchantInstallmentTransaction.ReturnStatus = ReturnStatus.Approved;
                _dbContext.Update(referenceMerchantInstallmentTransaction);
            }
        }
        else
        {
            merchantInstallmentTransaction.BankCommissionAmount = (merchantTransaction.BankCommissionRate / 100m) * amount;

            merchantInstallmentTransaction.PfCommissionAmount = pricingProfile.PerTransactionFee
                + pricingProfileItem.CommissionRate / 100m * amount;

            merchantInstallmentTransaction.PfNetCommissionAmount = merchantInstallmentTransaction.PfCommissionAmount
                - merchantInstallmentTransaction.BankCommissionAmount;

            merchantInstallmentTransaction.PfCommissionRate = pricingProfileItem.CommissionRate;
            merchantInstallmentTransaction.PfPerTransactionFee = pricingProfile.PerTransactionFee;

            merchantInstallmentTransaction.ParentMerchantCommissionAmount =
                pricingProfileItem.ParentMerchantCommissionRate / 100m * amount;

            merchantInstallmentTransaction.ParentMerchantCommissionRate =
                pricingProfileItem.ParentMerchantCommissionRate;

            merchantInstallmentTransaction.AmountWithoutCommissions = amount
                - merchantInstallmentTransaction.PfCommissionAmount
                - merchantInstallmentTransaction.ParentMerchantCommissionAmount;

            merchantInstallmentTransaction.AmountWithoutBankCommission = amount
                - merchantInstallmentTransaction.BankCommissionAmount;

            merchantInstallmentTransaction.AmountWithoutParentMerchantCommission = amount
                - merchantInstallmentTransaction.ParentMerchantCommissionAmount;


            merchantInstallmentTransaction.BsmvAmount = await BsmvAmountCalculateHelper
                .CalculateBsmvAmount(merchantInstallmentTransaction.PfNetCommissionAmount - merchantInstallmentTransaction.BankCommissionAmount, _parameterService);

            merchantInstallmentTransaction.PfPaymentDate = await _basePaymentService
                .CalculatePaymentDateAsync(merchantTransaction.TransactionDate, pricingBlockedDayNumber);

            merchantInstallmentTransaction.PointCommissionRate = merchantTransaction.PointCommissionRate;

            merchantInstallmentTransaction.PointCommissionAmount = merchantTransaction.PointCommissionAmount;

            merchantInstallmentTransaction.ServiceCommissionRate = merchantTransaction.ServiceCommissionRate;

            merchantInstallmentTransaction.ServiceCommissionAmount =
                (amount * merchantTransaction.ServiceCommissionRate) / 100m;

            merchantInstallmentTransaction.BankPaymentDate = merchantTransaction.TransactionDate.Date.AddDays(costBlockedDayNumber + 1);
        }

        return merchantInstallmentTransaction;
    }

    private async Task<bool> CalculateInstallmentTransactions(
    MerchantTransaction merchantTransaction,
    PhysicalPosRouteResponse routeResponse,
    Guid? referenceMerchantTransactionId,
    string vendor,
    decimal remaningAmount = 0)
    {
        try
        {
            decimal baseInstallmentAmount = Math.Floor(merchantTransaction.Amount / merchantTransaction.InstallmentCount * 100) / 100m;
            decimal totalDistributed = baseInstallmentAmount * merchantTransaction.InstallmentCount;
            decimal remainder = merchantTransaction.Amount - totalDistributed;

            var installmentTransactionList = new List<MerchantInstallmentTransaction>();

            for (int i = 1; i <= merchantTransaction.InstallmentCount; i++)
            {
                bool isFirstInstallment = i == 1;

                decimal installmentAmount = isFirstInstallment
                    ? baseInstallmentAmount + remainder
                    : baseInstallmentAmount;

                var pricingBlockedDayNumber = routeResponse.PricingProfileItem.PricingProfileInstallments.Where(b => b.InstallmentSequence == i).FirstOrDefault().BlockedDayNumber;
                var costBlockedDayNumber = routeResponse.Installments.Where(b => b.InstallmentSequence == i).FirstOrDefault().BlockedDayNumber;

                var referenceMerchantInstallmentTransaction = new MerchantInstallmentTransaction();
                if (referenceMerchantTransactionId != null)
                {
                    referenceMerchantInstallmentTransaction = await _merchantInstallmentTransactionRepository.GetAll().Where(b => b.MerchantTransactionId == referenceMerchantTransactionId && b.InstallmentCount == i).FirstOrDefaultAsync();
                }

                var installmentTransaction = await PopulateInitialMerchantInstallmentTransaction(merchantTransaction, i, installmentAmount, pricingBlockedDayNumber, costBlockedDayNumber, routeResponse.PricingProfileItem.PricingProfile, routeResponse.PricingProfileItem, referenceMerchantInstallmentTransaction, vendor, remaningAmount);

                installmentTransactionList.Add(installmentTransaction);
            }

            _dbContext.UpdateRange(installmentTransactionList);

        }
        catch (Exception exception)
        {
            _logger.LogError($"SavePhysicalPosMerchantInstallmentTransactionException: {exception}");
            return false;
        }


        return true;
    }
    private static BankTransaction PopulateInitialBankTransaction(PaxTransactionCommand request,
        MerchantTransaction merchantTransaction, MerchantPhysicalPos merchantPhysicalPos)
    {
        return new BankTransaction
        {
            TransactionType = merchantTransaction.TransactionType,
            TransactionStatus = merchantTransaction.TransactionStatus,
            OrderId = merchantTransaction.OrderId,
            Amount = merchantTransaction.Amount,
            PointAmount = merchantTransaction.PointAmount,
            Currency = merchantTransaction.Currency,
            InstallmentCount = merchantTransaction.InstallmentCount,
            CardNumber = merchantTransaction.CardNumber,
            IsReverse = false,
            Is3ds = false,
            IssuerBankCode = merchantTransaction.IssuerBankCode,
            AcquireBankCode = merchantTransaction.AcquireBankCode,
            MerchantCode = merchantPhysicalPos.PhysicalPos.PfMainMerchantId,
            SubMerchantCode = request.MerchantId,
            BankOrderId = request.BankRef,
            RrnNumber = request.Rrn,
            ApprovalCode = request.ProvisionNo,
            Stan = request.Stan,
            BankResponseCode = request.AcquirerResponseCode,
            BankTransactionDate = merchantTransaction.TransactionDate,
            TransactionStartDate = merchantTransaction.TransactionStartDate,
            TransactionEndDate = merchantTransaction.TransactionEndDate,
            VposId = Guid.Empty,
            MerchantTransactionId = merchantTransaction.Id,
            MerchantPhysicalPosId = merchantTransaction.MerchantPhysicalPosId,
            EndOfDayStatus = EndOfDayStatus.Pending
        };
    }

    private async Task<CreateOrUpdateEndOfDayResponse> CreateOrUpdateEodAsync(PaxTransactionCommand request, Merchant merchant,
        DateTime transactionDate)
    {
        var batchEod = await _dbContext.PhysicalPosEndOfDay.Where(s =>
            s.MerchantId == request.PfMerchantId &&
            s.PosMerchantId == request.MerchantId &&
            s.PosTerminalId == request.TerminalId &&
            s.BatchId == request.BatchId &&
            s.Currency == request.Currency
        ).FirstOrDefaultAsync();

        if (batchEod is null)
        {
            var eod = new PhysicalPosEndOfDay
            {
                MerchantId = request.PfMerchantId,
                MerchantName = merchant?.Name,
                MerchantNumber = merchant?.Number,
                BatchId = request.BatchId,
                PosMerchantId = request.MerchantId,
                PosTerminalId = request.TerminalId,
                Date = transactionDate.Date,
                SaleCount = request.Status == PaxTransactionStatus.Approved && request.Type == PaxTransactionType.Sale
                    ? 1
                    : 0,
                VoidCount = request.Status == PaxTransactionStatus.Approved && request.Type == PaxTransactionType.Void
                    ? 1
                    : 0,
                RefundCount = request.Status == PaxTransactionStatus.Approved &&
                              request.Type == PaxTransactionType.Refund
                    ? 1
                    : 0,
                InstallmentSaleCount = request.Status == PaxTransactionStatus.Approved &&
                                       request.Type == PaxTransactionType.InstallmentSale
                    ? 1
                    : 0,
                SaleAmount = request.Status == PaxTransactionStatus.Approved && request.Type == PaxTransactionType.Sale
                    ? request.Amount / 100m
                    : 0,
                VoidAmount = request.Status == PaxTransactionStatus.Approved && request.Type == PaxTransactionType.Void
                    ? request.Amount / 100m
                    : 0,
                RefundAmount =
                    request.Status == PaxTransactionStatus.Approved && request.Type == PaxTransactionType.Refund
                        ? request.Amount / 100m
                        : 0,
                InstallmentSaleAmount =
                    request.Status == PaxTransactionStatus.Approved &&
                    request.Type == PaxTransactionType.InstallmentSale
                        ? request.Amount / 100m
                        : 0,
                Currency = request.Currency,
                FailedCount = request.Status == PaxTransactionStatus.Declined ? 1 : 0,
                InstitutionId = request.InstitutionId,
                Vendor = request.Vendor,
                Status = EndOfDayStatus.Pending,
                SerialNumber = request.SerialNumber
            };

            return new CreateOrUpdateEndOfDayResponse
            {
                IsSucceeded = true,
                IsCreated = true,
                PhysicalPosEndOfDay = eod
            };
        }

        switch (request.Type)
        {
            case PaxTransactionType.Void when request.Status == PaxTransactionStatus.Approved:
                batchEod.VoidCount += 1;
                batchEod.VoidAmount += request.Amount / 100m;
                break;
            case PaxTransactionType.Refund when request.Status == PaxTransactionStatus.Approved:
                batchEod.RefundCount += 1;
                batchEod.RefundAmount += request.Amount / 100m;
                break;
            case PaxTransactionType.Sale when request.Status == PaxTransactionStatus.Approved:
                batchEod.SaleCount += 1;
                batchEod.SaleAmount += request.Amount / 100m;
                break;
            case PaxTransactionType.InstallmentSale when request.Status == PaxTransactionStatus.Approved:
                batchEod.InstallmentSaleCount += 1;
                batchEod.InstallmentSaleAmount += request.Amount / 100m;
                break;
        }

        batchEod.FailedCount = request.Status == PaxTransactionStatus.Declined
            ? batchEod.FailedCount + 1
            : batchEod.FailedCount;

        return new CreateOrUpdateEndOfDayResponse
        {
            IsSucceeded = true,
            IsCreated = false,
            PhysicalPosEndOfDay = batchEod
        };
    }

    private async Task<PaxTransactionResponse> AuthAsync(PaxTransactionCommand request,
        MerchantPhysicalPos merchantPhysicalPos, Merchant merchant, DateTime transactionDate)
    {
        // routeinfo ya gir
        var routeInfo = await _posRouterService.PhysicalPosRouteAsync(
            merchant,
            request.BinNumber,
            merchantPhysicalPos.PhysicalPosId,
            request.Installment,
            request.Amount / 100m,
            request.PointAmount / 100m,
            transactionDate,
            request.Currency,
            TransactionType.Auth);

        if (!routeInfo.IsSucceed)
        {
            return await UnacceptableAsync(request, routeInfo.ErrorCode);
        }

        var eodResponse = await CreateOrUpdateEodAsync(request, merchant, transactionDate);
        if (!eodResponse.IsSucceeded)
        {
            return await UnacceptableAsync(request, eodResponse.ErrorCode);
        }

        var eod = eodResponse.PhysicalPosEndOfDay;

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            if (eodResponse.IsCreated)
                await _dbContext.PhysicalPosEndOfDay.AddAsync(eod);
            else
                _dbContext.PhysicalPosEndOfDay.Update(eod);

            var merchantTransaction = await PopulateInitialMerchantTransactionAsync(request, merchantPhysicalPos,
                merchant,
                eod, routeInfo, transactionDate, null);
            var bankTransaction = PopulateInitialBankTransaction(request, merchantTransaction, merchantPhysicalPos);
            bankTransaction.PhysicalPosEodId = eod.Id;

            await _dbContext.MerchantTransaction.AddAsync(merchantTransaction);
            await _dbContext.BankTransaction.AddAsync(bankTransaction);

            if (merchantTransaction.TransactionStatus == TransactionStatus.Success && merchantTransaction.IsPerInstallment == true)
            {
                await CalculateInstallmentTransactions(merchantTransaction, routeInfo, null, request.Vendor, 0);

            }

            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });

        return new PaxTransactionResponse
        {
            IsSucceed = true
        };
    }

    private async Task<PaxTransactionResponse> ReturnAsync(PaxTransactionCommand request,
        MerchantPhysicalPos merchantPhysicalPos, Merchant merchant, DateTime transactionDate)
    {
        var referenceBankTransaction = await _dbContext.BankTransaction
            .FirstOrDefaultAsync(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.BankOrderId == request.OriginalRef &&
                s.SubMerchantCode == request.MerchantId &&
                s.AcquireBankCode == merchantPhysicalPos.PhysicalPos.AcquireBank.BankCode);

        if (referenceBankTransaction is null)
        {
            return await UnacceptableAsync(request, ApiErrorCode.InvalidReferenceNumber);
        }

        var referenceMerchantTransaction = await _dbContext.MerchantTransaction
            .Include(s => s.AcquireBank)
            .FirstOrDefaultAsync(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.PfTransactionSource == PfTransactionSource.PhysicalPos &&
                (s.TransactionStatus != TransactionStatus.Fail && s.TransactionStatus != TransactionStatus.Returned) &&
                s.Id == referenceBankTransaction.MerchantTransactionId &&
                s.MerchantId == merchant.Id &&
                !s.IsReverse);
        if (referenceMerchantTransaction is null)
        {
            return await UnacceptableAsync(request, ApiErrorCode.InvalidReferenceNumber);
        }

        if (referenceMerchantTransaction.IsChargeback || referenceMerchantTransaction.IsSuspecious)
        {
            return await UnacceptableAsync(request, ApiErrorCode.TransactionHasChargeback);
        }

        var totalRefundAmount = await _dbContext.MerchantTransaction
            .Where(s =>
                s.ReturnedTransactionId == referenceMerchantTransaction.Id.ToString() &&
                s.MerchantId == merchant.Id &&
                s.TransactionStatus == TransactionStatus.Success)
            .SumAsync(s => s.Amount);

        if ((totalRefundAmount + request.Amount / 100m) > referenceBankTransaction.Amount)
        {
            return await UnacceptableAsync(request, ApiErrorCode.InvalidReturnAmount);
        }

        var remainingReturnAmount = referenceBankTransaction.Amount - (totalRefundAmount + request.Amount / 100m);

        var routeInfo = await _posRouterService.PhysicalPosRouteAsync(
            merchant,
            request.BinNumber,
            merchantPhysicalPos.PhysicalPosId,
            request.Installment,
            request.Amount / 100m,
            request.PointAmount / 100m,
            transactionDate,
            request.Currency,
            TransactionType.Return,
            referenceMerchantTransaction,
            remainingReturnAmount);

        if (!routeInfo.IsSucceed)
        {
            return await UnacceptableAsync(request, routeInfo.ErrorCode);
        }

        var eodResponse = await CreateOrUpdateEodAsync(request, merchant, transactionDate);
        if (!eodResponse.IsSucceeded)
        {
            return await UnacceptableAsync(request, eodResponse.ErrorCode);
        }

        var eod = eodResponse.PhysicalPosEndOfDay;

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            if (eodResponse.IsCreated)
                await _dbContext.PhysicalPosEndOfDay.AddAsync(eod);
            else
                _dbContext.PhysicalPosEndOfDay.Update(eod);

            var merchantTransaction = await PopulateInitialMerchantTransactionAsync(request, merchantPhysicalPos,
                merchant,
                eod, routeInfo, transactionDate, referenceMerchantTransaction.ConversationId);
            merchantTransaction.ReturnedTransactionId = referenceMerchantTransaction.Id.ToString();
            var bankTransaction = PopulateInitialBankTransaction(request, merchantTransaction, merchantPhysicalPos);
            bankTransaction.PhysicalPosEodId = eod.Id;

            if (request.Status == PaxTransactionStatus.Approved)
            {
                var transactionStatus = (totalRefundAmount + request.Amount / 100m) == referenceBankTransaction.Amount
                    ? TransactionStatus.Returned
                    : TransactionStatus.PartiallyReturned;

                referenceMerchantTransaction.ReturnAmount = totalRefundAmount + request.Amount / 100m;
                referenceMerchantTransaction.ReturnDate = transactionDate;
                referenceMerchantTransaction.IsReturn = true;
                referenceMerchantTransaction.TransactionStatus = transactionStatus;
                referenceMerchantTransaction.LastModifiedBy = request.Vendor;
                referenceMerchantTransaction.ReturnStatus = ReturnStatus.Approved;
                _dbContext.Update(referenceMerchantTransaction);

                referenceBankTransaction.TransactionStatus = transactionStatus;
                referenceBankTransaction.LastModifiedBy = request.Vendor;
                _dbContext.Update(referenceBankTransaction);
            }

            await _dbContext.MerchantTransaction.AddAsync(merchantTransaction);
            await _dbContext.BankTransaction.AddAsync(bankTransaction);

            if (merchantTransaction.TransactionStatus == TransactionStatus.Success && merchantTransaction.IsPerInstallment == true)
            {
                await CalculateInstallmentTransactions(merchantTransaction, routeInfo, referenceMerchantTransaction.Id, request.Vendor, remainingReturnAmount);

            }

            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });

        return new PaxTransactionResponse
        {
            IsSucceed = true
        };
    }

    private async Task<PaxTransactionResponse> ReverseAsync(PaxTransactionCommand request,
        MerchantPhysicalPos merchantPhysicalPos,
        Merchant merchant, DateTime transactionDate)
    {
        var referenceBankTransaction = await _dbContext.BankTransaction
            .FirstOrDefaultAsync(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.Stan == request.OriginalRef &&
                s.SubMerchantCode == request.MerchantId &&
                s.AcquireBankCode == merchantPhysicalPos.PhysicalPos.AcquireBank.BankCode);

        if (referenceBankTransaction is null)
        {
            return await UnacceptableAsync(request, ApiErrorCode.InvalidReferenceNumber);
        }

        var referenceMerchantTransaction = await _dbContext.MerchantTransaction
            .Include(s => s.AcquireBank)
            .FirstOrDefaultAsync(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.PfTransactionSource == PfTransactionSource.PhysicalPos &&
                (s.TransactionStatus != TransactionStatus.Fail && s.TransactionStatus != TransactionStatus.Returned) &&
                s.Id == referenceBankTransaction.MerchantTransactionId &&
                s.MerchantId == merchant.Id &&
                !s.IsReverse);
        if (referenceMerchantTransaction is null)
        {
            return await UnacceptableAsync(request, ApiErrorCode.InvalidReferenceNumber);
        }

        if (referenceMerchantTransaction.IsChargeback || referenceMerchantTransaction.IsSuspecious)
        {
            return await UnacceptableAsync(request, ApiErrorCode.TransactionHasChargeback);
        }

        if (referenceMerchantTransaction.BatchStatus == BatchStatus.Completed)
        {
            return await UnacceptableAsync(request, ApiErrorCode.ReferenceTransactionPostingCompleted);
        }

        var bankTransaction = new BankTransaction
        {
            TransactionType = TransactionType.Reverse,
            TransactionStatus = request.Status switch
            {
                PaxTransactionStatus.Approved => TransactionStatus.Success,
                _ => TransactionStatus.Fail
            },
            OrderId = request.PaymentId,
            Amount = referenceBankTransaction.Amount,
            PointAmount = referenceBankTransaction.PointAmount,
            Currency = referenceBankTransaction.Currency,
            InstallmentCount = referenceBankTransaction.InstallmentCount,
            CardNumber = referenceBankTransaction.CardNumber,
            IsReverse = false,
            Is3ds = false,
            IssuerBankCode = referenceBankTransaction.IssuerBankCode,
            AcquireBankCode = referenceBankTransaction.AcquireBankCode,
            MerchantCode = referenceBankTransaction.MerchantCode,
            SubMerchantCode = referenceBankTransaction.SubMerchantCode,
            BankOrderId = request.BankRef,
            RrnNumber = request.Rrn,
            Stan = request.Stan,
            BankResponseCode = request.AcquirerResponseCode,
            BankTransactionDate = transactionDate,
            TransactionStartDate = transactionDate,
            TransactionEndDate = transactionDate,
            VposId = Guid.Empty,
            MerchantTransactionId = referenceMerchantTransaction.Id,
            MerchantPhysicalPosId = referenceMerchantTransaction.MerchantPhysicalPosId,
            EndOfDayStatus = EndOfDayStatus.Pending
        };

        var eodResponse = await CreateOrUpdateEodAsync(request, merchant, transactionDate);
        if (!eodResponse.IsSucceeded)
        {
            return await UnacceptableAsync(request, eodResponse.ErrorCode);
        }

        var eod = eodResponse.PhysicalPosEndOfDay;

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            if (eodResponse.IsCreated)
                await _dbContext.PhysicalPosEndOfDay.AddAsync(eod);
            else
                _dbContext.PhysicalPosEndOfDay.Update(eod);

            bankTransaction.PhysicalPosEodId = eod.Id;
            await _dbContext.AddAsync(bankTransaction);

            if (request.Status == PaxTransactionStatus.Approved)
            {
                referenceMerchantTransaction.IsReverse = true;
                referenceMerchantTransaction.ReverseDate = DateTime.Now;
                referenceMerchantTransaction.TransactionStatus = TransactionStatus.Reversed;
                referenceMerchantTransaction.LastModifiedBy = request.Vendor;
                referenceMerchantTransaction.PhysicalPosOldEodId = referenceMerchantTransaction.PhysicalPosEodId;
                referenceMerchantTransaction.PhysicalPosEodId = eod.Id;
                _dbContext.Update(referenceMerchantTransaction);

                referenceBankTransaction.IsReverse = true;
                referenceBankTransaction.ReverseDate = DateTime.Now;
                referenceBankTransaction.TransactionStatus = TransactionStatus.Reversed;
                referenceBankTransaction.LastModifiedBy = request.Vendor;
                _dbContext.Update(referenceBankTransaction);

                if (referenceMerchantTransaction.TransactionType == TransactionType.Return)
                {
                    var mainTransaction = await _dbContext.MerchantTransaction
                        .FirstOrDefaultAsync(s =>
                            s.Id == Guid.Parse(referenceMerchantTransaction.ReturnedTransactionId));

                    var successfullyReturnedTransactions = _dbContext.MerchantTransaction
                        .Where(s => s.TransactionType == TransactionType.Return &&
                                    s.TransactionStatus == TransactionStatus.Success &&
                                    s.ReturnedTransactionId == mainTransaction.Id.ToString());

                    var countOfReturnedTransactions = successfullyReturnedTransactions.Count();
                    var sumOfReturnedTransactionAmounts = successfullyReturnedTransactions.Sum(s => s.Amount);

                    var lastReturnTransaction = await successfullyReturnedTransactions
                        .Where(s => s.Id != referenceMerchantTransaction.Id)
                        .OrderByDescending(s => s.CreateDate)
                        .FirstOrDefaultAsync();

                    var sumOfReversedReturnTransactions = _dbContext.MerchantTransaction
                        .Where(s =>
                            s.TransactionType == TransactionType.Return &&
                            s.TransactionStatus == TransactionStatus.Reversed &&
                            s.ReturnedTransactionId == mainTransaction.Id.ToString())
                        .Sum(s => s.Amount);

                    var allAmountReversed = mainTransaction.Amount ==
                                            (referenceMerchantTransaction.Amount + sumOfReversedReturnTransactions);

                    if (allAmountReversed || countOfReturnedTransactions <= 1)
                    {
                        mainTransaction.TransactionStatus = TransactionStatus.Success;
                        mainTransaction.ReturnAmount = 0;
                        mainTransaction.IsReturn = false;
                    }
                    else
                    {
                        mainTransaction.ReturnAmount =
                            sumOfReturnedTransactionAmounts - referenceMerchantTransaction.Amount;
                        mainTransaction.TransactionStatus = TransactionStatus.PartiallyReturned;
                        mainTransaction.IsReturn = true;
                    }

                    mainTransaction.ReturnDate = (allAmountReversed || lastReturnTransaction is null)
                        ? DateTime.MinValue
                        : lastReturnTransaction.CreateDate;

                    mainTransaction.BankCommissionAmount = allAmountReversed
                        ? (mainTransaction.BankCommissionRate / 100m) * mainTransaction.Amount
                        : (mainTransaction.BankCommissionRate / 100m) *
                          (mainTransaction.Amount - mainTransaction.ReturnAmount);

                    _dbContext.Update(mainTransaction);
                }

                if (request.Status == PaxTransactionStatus.Approved && referenceMerchantTransaction.IsPerInstallment == true)
                {
                    var installmentTransactions = await _merchantInstallmentTransactionRepository.GetAll().Where(b => b.MerchantTransactionId == referenceMerchantTransaction.Id).ToListAsync();

                    //if return

                    if (installmentTransactions.Any())
                    {
                        foreach (var item in installmentTransactions)
                        {
                            item.IsReverse = true;
                            item.ReverseDate = DateTime.Now;
                            item.TransactionStatus = TransactionStatus.Reversed;
                            item.LastModifiedBy = request.Vendor;
                            item.PhysicalPosOldEodId = referenceMerchantTransaction.PhysicalPosEodId;
                            item.PhysicalPosEodId = eod.Id;
                        }

                        _dbContext.UpdateRange(installmentTransactions);
                    }
                }
            }

            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });

        return new PaxTransactionResponse
        {
            IsSucceed = true
        };
    }

    private async Task<PaxTransactionResponse> UnacceptableAsync(PaxTransactionCommand request, string code)
    {
        var merchant = await _dbContext.Merchant.FirstOrDefaultAsync(s => s.Id == request.PfMerchantId);
        var response = await GetErrorResponseAsync(code, request.ConversationId);
        var transactionDate = TransactionDateFromUnixTimestamp(request.Date);
        var eodResponse = await CreateOrUpdateEodAsync(request, merchant, transactionDate);
        var eod = eodResponse.PhysicalPosEndOfDay;
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            if (eodResponse.IsCreated)
                await _dbContext.PhysicalPosEndOfDay.AddAsync(eod);
            else
                _dbContext.PhysicalPosEndOfDay.Update(eod);
            var unacceptableTransaction = new PhysicalPosUnacceptableTransaction
            {
                PaymentId = request.PaymentId,
                BatchId = request.BatchId,
                Date = transactionDate,
                Type = request.Type,
                Status = request.Status,
                Currency = request.Currency,
                MerchantId = request.MerchantId,
                TerminalId = request.TerminalId,
                Amount = request.Amount / 100m,
                PointAmount = request.PointAmount / 100m,
                Installment = request.Installment,
                MaskedCardNo = request.MaskedCardNo,
                BinNumber = request.BinNumber,
                ProvisionNo = request.ProvisionNo,
                AcquirerResponseCode = request.AcquirerResponseCode,
                InstitutionId = request.InstitutionId,
                Vendor = request.Vendor,
                Rrn = request.Rrn,
                Stan = request.Stan,
                PosEntryMode = request.PosEntryMode,
                PinEntryInfo = request.PinEntryInfo,
                BankRef = request.BankRef,
                OriginalRef = request.OriginalRef,
                PfMerchantId = request.PfMerchantId,
                ConversationId = request.ConversationId,
                ClientIpAddress = request.ClientIpAddress,
                SerialNumber = request.SerialNumber,
                Gateway = request.Gateway,
                ErrorCode = response.ErrorCode,
                ErrorMessage = response.ErrorMessage,
                CurrentStatus = UnacceptableTransactionStatus.ActionRequired,
                PhysicalPosEodId = eod.Id,
                EndOfDayStatus = EndOfDayStatus.Pending,
                MerchantTransactionId = Guid.Empty
            };
            _dbContext.Add(unacceptableTransaction);
            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });


        await _bus.Publish(new PhysicalPosTransactionUnacceptable
        {
            Date = transactionDate,
            Vendor = request.Vendor,
            SerialNumber = request.SerialNumber,
            ErrorCode = response.ErrorCode,
            ErrorMessage = response.ErrorMessage,
            Type = request.Type,
            Status = request.Status,
            MerchantId = request.PfMerchantId,
            MerchantName = merchant?.Name,
            MerchantNumber = merchant?.Number,
            PaymentId = request.PaymentId,
            PosMerchantId = request.MerchantId,
            PosTerminalId = request.TerminalId,
            Amount = request.Amount / 100m,
            PointAmount = request.PointAmount / 100m,
            Currency = request.Currency,
            Installment = request.Installment,
            MaskedCardNo = request.MaskedCardNo,
            BankRef = request.BankRef,
            OriginalRef = request.OriginalRef,
            Rrn = request.Rrn
        }, CancellationToken.None);

        return response;
    }

    private static DateTime TransactionDateFromUnixTimestamp(long timestamp)
    {
        return timestamp switch
        {
            > 1_000_000_000_000_000 => DateTimeOffset.FromUnixTimeMilliseconds(timestamp / 1000).LocalDateTime,
            > 1_000_000_000_000 => DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime,
            _ => DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime
        };
    }

    private async Task<PaxParameterResponse> GetParameterErrorResponseAsync(string code, string conversationId)
    {
        var apiResponse = await _errorCodeService.GetApiResponseCode(code, "TR");

        return new PaxParameterResponse
        {
            IsSucceed = false,
            ErrorCode = code,
            ConversationId = conversationId,
            ErrorMessage = apiResponse.DisplayMessage,
        };
    }

    private static string BuildKey(int bankCode, string tid, string mid)
        => $"{bankCode}_{tid}_{mid}";

    private static void UpdateTerminalStatus(MerchantPhysicalPos pos, string state)
    {
        var newStatus = state == "1"
            ? DeviceTerminalStatus.Active
            : DeviceTerminalStatus.Passive;

        if (pos.DeviceTerminalStatus != newStatus)
            pos.DeviceTerminalLastActivity = DateTime.Now;

        pos.DeviceTerminalStatus = newStatus;
    }

    public async Task<PaxParameterResponse> ParametersAsync(PaxParameterCommand request)
    {
        try
        {
            if (request.Gateway != nameof(Gateway.PFPosGateway))
            {
                return await GetParameterErrorResponseAsync(ApiErrorCode.InvalidGateway, request.ConversationId);
            }

            var merchant = await _dbContext.Merchant.Where(s => s.Id == request.PfMerchantId).FirstOrDefaultAsync();
            if (merchant is null)
            {
                return await GetParameterErrorResponseAsync(ApiErrorCode.InvalidMerchant, request.ConversationId);
            }

            var device = await _dbContext.DeviceInventory.Where(s => s.SerialNo == request.SerialNumber)
                .FirstOrDefaultAsync();
            if (device is null)
            {
                return await GetParameterErrorResponseAsync(ApiErrorCode.PhysicalDeviceNotFound, request.ConversationId);
            }

            var merchantPhysicalDevice =
                await _dbContext.MerchantPhysicalDevice
                    .Where(s => s.DeviceInventoryId == device.Id && s.MerchantId == request.PfMerchantId)
                    .FirstOrDefaultAsync();
            if (merchantPhysicalDevice is null || merchantPhysicalDevice.RecordStatus != RecordStatus.Active)
            {
                return await GetParameterErrorResponseAsync(ApiErrorCode.MerchantPhysicalDeviceNotFound, request.ConversationId);
            }

            var merchantPhysicalPosList = await _dbContext.MerchantPhysicalPos
                .Include(s => s.PhysicalPos)
                .ThenInclude(a => a.AcquireBank)
                .ThenInclude(a => a.Bank)
                .Where(s => s.MerchantPhysicalDeviceId == merchantPhysicalDevice.Id)
                .ToListAsync();

            var posLookup = merchantPhysicalPosList.ToDictionary(
                x => BuildKey(
                    x.PhysicalPos.AcquireBank.BankCode,
                    x.PosTerminalId,
                    x.PosMerchantId),
                x => x
            );

            var foundIds = new HashSet<Guid>();

            foreach (var appInfo in request.AppInfo)
            {
                var key = BuildKey(
                    int.Parse(appInfo.AcqId),
                    appInfo.Tid,
                    appInfo.Mid
                );

                if (!posLookup.TryGetValue(key, out var pos))
                {
                    await _bus.Publish(new DeviceTerminalNotFound
                    {
                        BankName = appInfo.Name,
                        AcqId = appInfo.AcqId,
                        Tid = appInfo.Tid,
                        Mid = appInfo.Mid,
                        Date = DateTime.Now,
                        SerialNumber = request.SerialNumber,
                        MerchantNumber = merchant.Number,
                        MerchantName = merchant.Name
                    }, CancellationToken.None);
                    continue;
                }

                foundIds.Add(pos.Id);
                UpdateTerminalStatus(pos, appInfo.State);
            }

            var activeNotFoundList = merchantPhysicalPosList
                .Where(x => !foundIds.Contains(x.Id) &&
                            x.DeviceTerminalStatus == DeviceTerminalStatus.Active).ToList();

            foreach (var pos in activeNotFoundList)
            {
                await _bus.Publish(new ActiveTerminalMissingOnDevice
                {
                    BankName = pos.PhysicalPos.AcquireBank.Bank.Name,
                    AcqId = pos.PhysicalPos.AcquireBank.BankCode.ToString(),
                    Tid = pos.PosTerminalId,
                    Mid = pos.PosMerchantId,
                    Date = DateTime.Now,
                    SerialNumber = request.SerialNumber,
                    MerchantNumber = merchant.Number,
                    MerchantName = merchant.Name
                }, CancellationToken.None);
            }

            _dbContext.Update(merchantPhysicalPosList);
            await _dbContext.SaveChangesAsync();

            return new PaxParameterResponse
            {
                IsSucceed = true
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Pax Parameter Error: {exception}");
            return new PaxParameterResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId
            };
        }
    }

    private static bool IsEmptyEndOfDayRequest(PaxEndOfDayCommand request)
    {
        return request.VoidCount == 0 && request.VoidAmount == 0 &&
               request.SaleCount == 0 && request.SaleAmount == 0 &&
               request.InstallmentSaleCount == 0 && request.InstallmentSaleAmount == 0 &&
               request.RefundCount == 0 && request.RefundAmount == 0;
    }

    public async Task<PaxEndOfDayResponse> EndOfDayAsync(PaxEndOfDayCommand request)
    {
        try
        {
            var transactionDate = TransactionDateFromUnixTimestamp(request.Date);

            var batchEod = await _dbContext.PhysicalPosEndOfDay.Where(s =>
                s.MerchantId == request.PfMerchantId &&
                s.PosMerchantId == request.MerchantId &&
                s.PosTerminalId == request.TerminalId &&
                s.BatchId == request.BatchId
            ).FirstOrDefaultAsync();

            var merchant = await _dbContext.Merchant.FirstOrDefaultAsync(s => s.Id == request.PfMerchantId);
            if (batchEod is null)
            {
                if (IsEmptyEndOfDayRequest(request))
                {
                    batchEod = new PhysicalPosEndOfDay
                    {
                        MerchantId = request.PfMerchantId,
                        MerchantName = merchant?.Name,
                        MerchantNumber = merchant?.Number,
                        BatchId = request.BatchId,
                        PosMerchantId = request.MerchantId,
                        PosTerminalId = request.TerminalId,
                        Date = transactionDate.Date,
                        SaleCount = request.SaleCount,
                        VoidCount = request.VoidCount,
                        RefundCount = request.RefundCount,
                        InstallmentSaleCount = request.InstallmentSaleCount,
                        SaleAmount = request.SaleAmount / 100m,
                        VoidAmount = request.VoidAmount / 100m,
                        RefundAmount = request.RefundAmount / 100m,
                        InstallmentSaleAmount = request.InstallmentSaleAmount / 100m,
                        Currency = request.Currency,
                        FailedCount = 0,
                        InstitutionId = request.InstitutionId,
                        Vendor = request.Vendor,
                        Status = EndOfDayStatus.Empty,
                        SerialNumber = request.SerialNumber
                    };
                    _dbContext.PhysicalPosEndOfDay.Add(batchEod);
                    await _dbContext.SaveChangesAsync();

                    return new PaxEndOfDayResponse
                    {
                        IsSucceed = true,
                        ConversationId = request.ConversationId
                    };
                }
                else
                {
                    await PublishPhysicalPosReconciliationOccuredAsync(request, transactionDate);

                    return new PaxEndOfDayResponse
                    {
                        IsSucceed = false,
                        ErrorCode = GenericErrorCode,
                        ErrorMessage = "InternalError",
                        ConversationId = request.ConversationId
                    };
                }
            }

            var acceptableTransactionStatusList =
                new List<TransactionStatus>
                {
                    TransactionStatus.Success, TransactionStatus.Returned, TransactionStatus.PartiallyReturned,
                    TransactionStatus.Reversed
                };
            var bankTransactions =
                await _dbContext.BankTransaction.Where(s =>
                        s.PhysicalPosEodId == batchEod.Id &&
                        acceptableTransactionStatusList.Contains(s.TransactionStatus) &&
                        s.TransactionStatus != TransactionStatus.Fail && s.TransactionStatus != TransactionStatus.Pending
                        )
                    .ToListAsync();

            var merchantTransactionIds = bankTransactions.Select(s => s.MerchantTransactionId).ToList();
            var merchantTransactions = await _dbContext.MerchantTransaction
                .Where(s => merchantTransactionIds.Contains(s.Id)).ToListAsync();

            var unacceptableTransactions = await _dbContext.PhysicalPosUnacceptableTransaction
                .Where(s =>
                    s.CurrentStatus != UnacceptableTransactionStatus.Accepted &&
                    s.Status == PaxTransactionStatus.Approved &&
                    s.PfMerchantId == request.PfMerchantId &&
                    s.MerchantId == request.MerchantId &&
                    s.TerminalId == request.TerminalId &&
                    s.BatchId == request.BatchId)
                .ToListAsync();

            var eodCalculation = new EndOfDayCalculation
            {
                SaleCount = bankTransactions.Count(s =>
                                s.TransactionType == TransactionType.Auth && s.InstallmentCount <= 1) +
                            unacceptableTransactions.Count(s => s.Type == PaxTransactionType.Sale),
                VoidCount = bankTransactions.Count(s => s.TransactionType == TransactionType.Reverse) +
                            unacceptableTransactions.Count(s => s.Type == PaxTransactionType.Void),
                RefundCount = bankTransactions.Count(s => s.TransactionType == TransactionType.Return) +
                              unacceptableTransactions.Count(s => s.Type == PaxTransactionType.Refund),
                InstallmentSaleCount =
                    bankTransactions.Count(s => s.TransactionType == TransactionType.Auth && s.InstallmentCount > 1) +
                    unacceptableTransactions.Count(s => s.Type == PaxTransactionType.InstallmentSale),
                SaleAmount = bankTransactions
                                 .Where(s => s.TransactionType == TransactionType.Auth && s.InstallmentCount <= 1)
                                 .Sum(s => s.Amount) +
                             unacceptableTransactions.Where(s => s.Type == PaxTransactionType.Sale).Sum(s => s.Amount),
                VoidAmount = bankTransactions.Where(s => s.TransactionType == TransactionType.Reverse)
                                 .Sum(s => s.Amount) +
                             unacceptableTransactions.Where(s => s.Type == PaxTransactionType.Void).Sum(s => s.Amount),
                RefundAmount = bankTransactions.Where(s => s.TransactionType == TransactionType.Return)
                                   .Sum(s => s.Amount) +
                               unacceptableTransactions.Where(s => s.Type == PaxTransactionType.Refund)
                                   .Sum(s => s.Amount),
                InstallmentSaleAmount = bankTransactions
                                            .Where(s => s.TransactionType == TransactionType.Auth &&
                                                        s.InstallmentCount > 1).Sum(s => s.Amount) +
                                        unacceptableTransactions
                                            .Where(s => s.Type == PaxTransactionType.InstallmentSale).Sum(s => s.Amount)
            };

            var isEodSuccessful = request.SaleCount == eodCalculation.SaleCount &&
                                  request.VoidCount == eodCalculation.VoidCount &&
                                  request.RefundCount == eodCalculation.RefundCount &&
                                  request.InstallmentSaleCount == eodCalculation.InstallmentSaleCount &&
                                  request.SaleAmount / 100m == eodCalculation.SaleAmount &&
                                  request.VoidAmount / 100m == eodCalculation.VoidAmount &&
                                  request.RefundAmount / 100m == eodCalculation.RefundAmount &&
                                  request.InstallmentSaleAmount / 100m == eodCalculation.InstallmentSaleAmount;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (isEodSuccessful)
                {
                    var eodSuspendedBankTransactionIds = new List<Guid>();
                    var voidUnacceptableOriginalRefList =
                        unacceptableTransactions
                            .Where(s => s.Type == PaxTransactionType.Void)
                            .Select(s => s.OriginalRef)
                            .ToList();
                    if (voidUnacceptableOriginalRefList.Count > 0)
                    {
                        var suspendedBankTransactionIds = bankTransactions
                            .Where(s => voidUnacceptableOriginalRefList.Contains(s.BankOrderId))
                            .Select(s => s.Id)
                            .ToList();
                        eodSuspendedBankTransactionIds.AddRange(suspendedBankTransactionIds);
                    }

                    var successfulMerchantTransactionIds = bankTransactions
                        .Where(s => !eodSuspendedBankTransactionIds.Contains(s.Id))
                        .Select(s => s.MerchantTransactionId).ToList();
                    var successfulMerchantTransactions = merchantTransactions
                        .Where(s => successfulMerchantTransactionIds.Contains(s.Id)).ToList();
                    successfulMerchantTransactions.ForEach(s =>
                    {
                        if (s.BatchStatus == BatchStatus.EodPending)
                            s.BatchStatus = BatchStatus.Pending;
                        s.EndOfDayStatus = EndOfDayStatus.Completed;
                    });
                    _dbContext.MerchantTransaction.UpdateRange(successfulMerchantTransactions);

                    var suspendedMerchantTransactionIds = bankTransactions
                        .Where(s => eodSuspendedBankTransactionIds.Contains(s.Id))
                        .Select(s => s.MerchantTransactionId).ToList();
                    var suspendedMerchantTransactions = merchantTransactions
                        .Where(s => suspendedMerchantTransactionIds.Contains(s.Id)).ToList();
                    suspendedMerchantTransactions.ForEach(s => { s.EndOfDayStatus = EndOfDayStatus.Suspended; });
                    _dbContext.MerchantTransaction.UpdateRange(suspendedMerchantTransactions);

                    var successfulBankTransactions = bankTransactions
                        .Where(s => !eodSuspendedBankTransactionIds.Contains(s.Id)).ToList();
                    successfulBankTransactions.ForEach(s => { s.EndOfDayStatus = EndOfDayStatus.Completed; });
                    _dbContext.BankTransaction.UpdateRange(successfulBankTransactions);

                    var suspendedBankTransactions =
                        bankTransactions.Where(s => eodSuspendedBankTransactionIds.Contains(s.Id)).ToList();
                    suspendedBankTransactions.ForEach(s => { s.EndOfDayStatus = EndOfDayStatus.Suspended; });
                    _dbContext.BankTransaction.UpdateRange(suspendedBankTransactions);

                    unacceptableTransactions.ForEach(s =>
                    {
                        s.EndOfDayStatus = EndOfDayStatus.Completed;
                        s.PhysicalPosEodId = batchEod.Id;
                    });
                    _dbContext.PhysicalPosUnacceptableTransaction.UpdateRange(unacceptableTransactions);

                    batchEod.SaleCount = eodCalculation.SaleCount;
                    batchEod.VoidCount = eodCalculation.VoidCount;
                    batchEod.RefundCount = eodCalculation.RefundCount;
                    batchEod.InstallmentSaleCount = eodCalculation.InstallmentSaleCount;
                    batchEod.SaleAmount = eodCalculation.SaleAmount;
                    batchEod.VoidAmount = eodCalculation.VoidAmount;
                    batchEod.RefundAmount = eodCalculation.RefundAmount;
                    batchEod.InstallmentSaleAmount = eodCalculation.InstallmentSaleAmount;
                    batchEod.Status = unacceptableTransactions.Count > 0
                        ? EndOfDayStatus.ActionRequired
                        : EndOfDayStatus.Completed;
                }
                else
                {
                    var relatedMerchantTransactions = bankTransactions.Select(s => s.MerchantTransactionId).ToList();
                    await _dbContext
                        .MerchantTransaction
                        .Where(a => relatedMerchantTransactions.Contains(a.Id))
                        .ExecuteUpdateAsync(u => u
                            .SetProperty(p => p.EndOfDayStatus, EndOfDayStatus.Reconciliation)
                        );

                    bankTransactions.ForEach(s => { s.EndOfDayStatus = EndOfDayStatus.Reconciliation; });
                    _dbContext.BankTransaction.UpdateRange(bankTransactions);

                    unacceptableTransactions.ForEach(s =>
                    {
                        s.EndOfDayStatus = EndOfDayStatus.Reconciliation;
                        s.PhysicalPosEodId = batchEod.Id;
                    });
                    _dbContext.PhysicalPosUnacceptableTransaction.UpdateRange(unacceptableTransactions);

                    batchEod.Status = EndOfDayStatus.Reconciliation;
                }

                _dbContext.PhysicalPosEndOfDay.Update(batchEod);
                await _dbContext.SaveChangesAsync();
                scope.Complete();
            });

            if (batchEod.Status == EndOfDayStatus.ActionRequired)
            {
                await PublishEndOfDayActionRequiredAsync(request, unacceptableTransactions, transactionDate);
            }

            if (isEodSuccessful)
            {
                return new PaxEndOfDayResponse
                {
                    IsSucceed = true,
                    ConversationId = request.ConversationId
                };
            }

            await PublishPhysicalPosReconciliationOccuredAsync(request, transactionDate);

            return new PaxEndOfDayResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Pax EndOfDay Error: {exception}");
            return new PaxEndOfDayResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId
            };
        }
    }

    private async Task PublishEndOfDayActionRequiredAsync(PaxEndOfDayCommand request,
        List<PhysicalPosUnacceptableTransaction> unacceptableTransactions, DateTime transactionDate)
    {
        var merchant = await _dbContext.Merchant.FirstOrDefaultAsync(s => s.Id == request.PfMerchantId);
        await _bus.Publish(new PhysicalPosEndOfDayActionRequired
        {
            Date = transactionDate,
            Vendor = request.Vendor,
            SerialNumber = request.SerialNumber,
            MerchantId = request.PfMerchantId,
            MerchantName = merchant?.Name,
            MerchantNumber = merchant?.Number,
            PosMerchantId = request.MerchantId,
            PosTerminalId = request.TerminalId,
            BatchId = request.BatchId,
            BankId = request.BankId,
            SaleCount = request.SaleCount,
            VoidCount = request.VoidCount,
            RefundCount = request.RefundCount,
            InstallmentSaleCount = request.InstallmentSaleCount,
            SaleAmount = request.SaleAmount / 100m,
            VoidAmount = request.VoidAmount / 100m,
            RefundAmount = request.RefundAmount / 100m,
            InstallmentSaleAmount = request.InstallmentSaleAmount / 100m,
            Currency = request.Currency,
            UnacceptableSaleAmount = unacceptableTransactions.Where(s => s.Type == PaxTransactionType.Sale)
                .Select(s => s.Amount).Sum(),
            UnacceptableSaleCount = unacceptableTransactions.Count(s => s.Type == PaxTransactionType.Sale),
            UnacceptableInstallmentSaleAmount = unacceptableTransactions
                .Where(s => s.Type == PaxTransactionType.InstallmentSale).Select(s => s.Amount).Sum(),
            UnacceptableInstallmentSaleCount =
                unacceptableTransactions.Count(s => s.Type == PaxTransactionType.InstallmentSale),
            UnacceptableVoidAmount = unacceptableTransactions.Where(s => s.Type == PaxTransactionType.Void)
                .Select(s => s.Amount).Sum(),
            UnacceptableVoidCount = unacceptableTransactions.Count(s => s.Type == PaxTransactionType.Void),
            UnacceptableRefundAmount = unacceptableTransactions.Where(s => s.Type == PaxTransactionType.Refund)
                .Select(s => s.Amount).Sum(),
            UnacceptableRefundCount = unacceptableTransactions.Count(s => s.Type == PaxTransactionType.Refund),
        }, CancellationToken.None);
    }

    private async Task PublishPhysicalPosReconciliationOccuredAsync(PaxEndOfDayCommand request,
        DateTime transactionDate)
    {
        var merchant = await _dbContext.Merchant.FirstOrDefaultAsync(s => s.Id == request.PfMerchantId);
        await _bus.Publish(new PhysicalPosReconciliationOccured
        {
            Date = transactionDate,
            Vendor = request.Vendor,
            SerialNumber = request.SerialNumber,
            MerchantId = request.PfMerchantId,
            MerchantName = merchant?.Name,
            MerchantNumber = merchant?.Number,
            PosMerchantId = request.MerchantId,
            PosTerminalId = request.TerminalId,
            BatchId = request.BatchId,
            BankId = request.BankId,
            SaleCount = request.SaleCount,
            VoidCount = request.VoidCount,
            RefundCount = request.RefundCount,
            InstallmentSaleCount = request.InstallmentSaleCount,
            SaleAmount = request.SaleAmount / 100m,
            VoidAmount = request.VoidAmount / 100m,
            RefundAmount = request.RefundAmount / 100m,
            InstallmentSaleAmount = request.InstallmentSaleAmount / 100m,
            Currency = request.Currency,
        }, CancellationToken.None);
    }

    public async Task<PaxReconciliationResponse> ReconciliationAsync(PaxReconciliationCommand request)
    {
        try
        {
            var transactionDate = TransactionDateFromUnixTimestamp(request.Date);

            var batchEod = await _dbContext.PhysicalPosEndOfDay.Where(s =>
                s.MerchantId == request.PfMerchantId &&
                s.PosMerchantId == request.MerchantId &&
                s.PosTerminalId == request.TerminalId &&
                s.BatchId == request.BatchId
            ).FirstOrDefaultAsync();

            var reconciliationTransactions = new List<PhysicalPosReconciliationTransaction>();

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (batchEod is null)
                {
                    var merchant = await _dbContext.Merchant.FirstOrDefaultAsync(s => s.Id == request.PfMerchantId);
                    batchEod = new PhysicalPosEndOfDay
                    {
                        MerchantId = request.PfMerchantId,
                        MerchantName = merchant?.Name,
                        MerchantNumber = merchant?.Number,
                        BatchId = request.BatchId,
                        PosMerchantId = request.MerchantId,
                        PosTerminalId = request.TerminalId,
                        Date = transactionDate.Date,
                        SaleCount = request.SaleCount,
                        VoidCount = request.VoidCount,
                        RefundCount = request.RefundCount,
                        InstallmentSaleCount = request.InstallmentSaleCount,
                        SaleAmount = request.SaleAmount / 100m,
                        VoidAmount = request.VoidAmount / 100m,
                        RefundAmount = request.RefundAmount / 100m,
                        InstallmentSaleAmount = request.InstallmentSaleAmount / 100m,
                        Currency = request.Currency,
                        FailedCount = 0,
                        InstitutionId = request.InstitutionId,
                        Vendor = request.Vendor,
                        Status = EndOfDayStatus.Reconciliation,
                        SerialNumber = request.SerialNumber
                    };

                    _dbContext.PhysicalPosEndOfDay.Add(batchEod);
                }
                else
                {
                    batchEod.SaleCount = request.SaleCount;
                    batchEod.VoidCount = request.VoidCount;
                    batchEod.RefundCount = request.RefundCount;
                    batchEod.InstallmentSaleCount = request.InstallmentSaleCount;
                    batchEod.SaleAmount = request.SaleAmount / 100m;
                    batchEod.VoidAmount = request.VoidAmount / 100m;
                    batchEod.RefundAmount = request.RefundAmount / 100m;
                    batchEod.InstallmentSaleAmount = request.InstallmentSaleAmount / 100m;
                    _dbContext.PhysicalPosEndOfDay.Update(batchEod);
                }

                var checkExistedTransactions = await _dbContext.PhysicalPosReconciliationTransaction
                    .Where(s => s.PhysicalPosEodId == batchEod.Id).ToListAsync();
                if (checkExistedTransactions.Count > 0)
                {
                    checkExistedTransactions.ForEach(s => { s.RecordStatus = RecordStatus.Passive; });
                    _dbContext.UpdateRange(checkExistedTransactions);
                }

                reconciliationTransactions =
                    request.Transactions.Select(s => new PhysicalPosReconciliationTransaction
                    {
                        PaymentId = s.PaymentId,
                        BatchId = s.BatchId,
                        Date = TransactionDateFromUnixTimestamp(s.Date),
                        Type = s.Type,
                        Status = s.Status,
                        Currency = s.Currency,
                        MerchantId = s.MerchantId,
                        TerminalId = s.TerminalId,
                        Amount = s.Amount / 100m,
                        PointAmount = s.PointAmount / 100m,
                        Installment = s.Installment,
                        MaskedCardNo = s.MaskedCardNo,
                        BinNumber = s.BinNumber,
                        ProvisionNo = s.ProvisionNo,
                        AcquirerResponseCode = s.AcquirerResponseCode,
                        InstitutionId = s.InstitutionId,
                        Vendor = s.Vendor,
                        Rrn = s.Rrn,
                        Stan = s.Stan,
                        PosEntryMode = s.PosEntryMode,
                        PinEntryInfo = s.PinEntryInfo,
                        BankRef = s.BankRef,
                        OriginalRef = s.OriginalRef,
                        PfMerchantId = request.PfMerchantId,
                        ConversationId = request.ConversationId,
                        ClientIpAddress = request.ClientIpAddress,
                        SerialNumber = request.SerialNumber,
                        ReconciliationStatus = ReconciliationStatus.Pending,
                        MerchantTransactionId = Guid.Empty,
                        UnacceptableTransactionId = Guid.Empty,
                        PhysicalPosEodId = batchEod.Id
                    }).ToList();

                await _dbContext.PhysicalPosReconciliationTransaction.AddRangeAsync(reconciliationTransactions);
                await _dbContext.SaveChangesAsync();
                scope.Complete();
            });

            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.PhysicalPosReconciliation"));
            await endpoint.Send(new PhysicalPosReconciliation
            {
                PhysicalPosEodId = batchEod.Id,
                ReconciliationTransactionIds = reconciliationTransactions.Select(s => s.Id).ToList(),
            }, tokenSource.Token);

            return new PaxReconciliationResponse
            {
                IsSucceed = true,
                ConversationId = request.ConversationId
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Pax EndOfDay Reconciliation Error: {exception}");
            return new PaxReconciliationResponse
            {
                IsSucceed = false,
                ErrorCode = GenericErrorCode,
                ErrorMessage = "InternalError",
                ConversationId = request.ConversationId
            };
        }
    }

    private async Task<PaxTransactionResponse> GetErrorResponseAsync(string code, string conversationId)
    {
        var apiResponse = await _errorCodeService.GetApiResponseCode(code, "TR");

        return new PaxTransactionResponse
        {
            IsSucceed = true,
            ErrorCode = code,
            ConversationId = conversationId,
            ErrorMessage = apiResponse.DisplayMessage,
        };
    }

    public async Task RetryTransactionAsync(PhysicalPosUnacceptableTransaction unacceptableTransaction)
    {
        try
        {
            var merchant = await _dbContext.Merchant.Where(s => s.Id == unacceptableTransaction.PfMerchantId)
                .FirstOrDefaultAsync();
            if (merchant is null)
            {
                throw new NotFoundException(nameof(Merchant), unacceptableTransaction.PfMerchantId);
            }

            if (merchant.MerchantStatus != MerchantStatus.Active)
            {
                throw new InvalidMerchantStatusException();
            }

            var activeMerchantTransaction = await _dbContext.MerchantTransaction
                .FirstOrDefaultAsync(b => b.MerchantId == unacceptableTransaction.PfMerchantId
                                          && b.OrderId == unacceptableTransaction.PaymentId
                                          && b.RecordStatus == RecordStatus.Active);
            if (activeMerchantTransaction is not null)
            {
                throw new DuplicateMerchantTransactionException();
            }

            var device = await _dbContext.DeviceInventory.Where(s => s.SerialNo == unacceptableTransaction.SerialNumber)
                .FirstOrDefaultAsync();
            if (device is null)
            {
                throw new NotFoundException(nameof(DeviceInventory), unacceptableTransaction.SerialNumber);
            }

            var merchantPhysicalDevice =
                await _dbContext.MerchantPhysicalDevice
                    .Where(s => s.DeviceInventoryId == device.Id &&
                                s.MerchantId == unacceptableTransaction.PfMerchantId)
                    .FirstOrDefaultAsync();
            if (merchantPhysicalDevice is null || merchantPhysicalDevice.RecordStatus != RecordStatus.Active)
            {
                throw new NotFoundException(nameof(MerchantPhysicalDevice), unacceptableTransaction.SerialNumber);
            }

            var merchantPhysicalPos = await _dbContext.MerchantPhysicalPos
                .Include(s => s.PhysicalPos)
                .ThenInclude(a => a.AcquireBank)
                .Include(s => s.PhysicalPos.Currencies)
                .Where(s =>
                    s.MerchantPhysicalDeviceId == merchantPhysicalDevice.Id &&
                    s.PosMerchantId == unacceptableTransaction.MerchantId &&
                    s.PosTerminalId == unacceptableTransaction.TerminalId &&
                    s.RecordStatus == RecordStatus.Active &&
                    s.PhysicalPos.Currencies.Any(a => a.CurrencyCode == unacceptableTransaction.Currency))
                .FirstOrDefaultAsync();
            if (merchantPhysicalPos is null)
            {
                throw new NotFoundException(nameof(MerchantPhysicalPos), unacceptableTransaction.TerminalId);
            }

            switch (unacceptableTransaction.Type)
            {
                case PaxTransactionType.Sale or PaxTransactionType.InstallmentSale:
                    await AuthRetryAsync(unacceptableTransaction, merchantPhysicalPos, merchant);
                    break;
                case PaxTransactionType.Refund:
                    await ReturnRetryAsync(unacceptableTransaction, merchantPhysicalPos, merchant);
                    break;
                case PaxTransactionType.Void:
                    await ReverseRetryAsync(unacceptableTransaction, merchantPhysicalPos, merchant);
                    break;
                default:
                    throw new InvalidTransactionTypeException();
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"Pax Retry Transaction Error: {exception}");
            throw;
        }
    }

    private async Task<MerchantTransaction> PopulateMerchantTransactionFromUnacceptableAsync(
        PhysicalPosUnacceptableTransaction unacceptableTransaction,
        MerchantPhysicalPos merchantPhysicalPos,
        Merchant merchant,
        PhysicalPosEndOfDay eod,
        PhysicalPosRouteResponse routeInfo,
        string conversationId)
    {
        return new MerchantTransaction
        {
            MerchantId = merchant.Id,
            ConversationId = conversationId ??
                             await _orderNumberGeneratorService.GenerateForPhysicalPosTransactionAsync(
                                 routeInfo.AcquireBank.BankCode, merchant.Number, merchant.Name),
            IpAddress = unacceptableTransaction.ClientIpAddress,
            TransactionType = unacceptableTransaction.Type switch
            {
                PaxTransactionType.Sale or PaxTransactionType.InstallmentSale => TransactionType.Auth,
                PaxTransactionType.Void => TransactionType.Reverse,
                PaxTransactionType.Refund => TransactionType.Return,
                _ => TransactionType.Auth
            },
            TransactionStatus = unacceptableTransaction.Status switch
            {
                PaxTransactionStatus.Approved => TransactionStatus.Success,
                _ => TransactionStatus.Fail
            },
            OrderId = unacceptableTransaction.PaymentId,
            Amount = routeInfo.Amount,
            PointAmount = routeInfo.PointAmount,
            PointCommissionRate = routeInfo.PointCommissionRate,
            PointCommissionAmount = routeInfo.PointCommissionAmount,
            ServiceCommissionRate = routeInfo.ServiceCommissionRate,
            ServiceCommissionAmount = routeInfo.ServiceCommissionAmount,
            Currency = routeInfo.CurrencyNumber,
            InstallmentCount = unacceptableTransaction.Installment,
            BinNumber = unacceptableTransaction.BinNumber,
            CardNumber = unacceptableTransaction.MaskedCardNo,
            HasCvv = false,
            HasExpiryDate = false,
            IsInternational = routeInfo.IsInternational,
            IsAmex = routeInfo.IsAmex,
            IsReverse = false,
            IsManualReturn = false,
            IsOnUsPayment = false,
            IsInsurancePayment = false,
            IsReturn = false,
            ReturnAmount = 0,
            IsPreClose = false,
            Is3ds = false,
            BankCommissionRate = routeInfo.BankCommissionRate,
            BankCommissionAmount = routeInfo.BankCommissionAmount,
            IssuerBankCode = routeInfo.IssuerBankCode,
            AcquireBankCode = routeInfo.AcquireBank.BankCode,
            CardTransactionType = routeInfo.CardTransactionType,
            IntegrationMode = IntegrationMode.Api,
            ResponseCode = GenericSuccessCode,
            ResponseDescription = GenericSuccessCode,
            TransactionStartDate = unacceptableTransaction.Date,
            TransactionEndDate = unacceptableTransaction.Date,
            VposId = Guid.Empty,
            LanguageCode = "TR",
            BatchStatus = unacceptableTransaction.EndOfDayStatus == EndOfDayStatus.Completed
                ? BatchStatus.Pending
                : BatchStatus.EodPending,
            CardType = routeInfo.CardType,
            TransactionDate = unacceptableTransaction.Date,
            IsChargeback = false,
            IsTopUpPayment = false,
            IsSuspecious = false,
            LastChargebackActivityDate = DateTime.MinValue,
            Description = $"InstitutionId:{unacceptableTransaction.InstitutionId}, " +
                          $"PosEntryMode:{PaxPosEntryMode.GetName(unacceptableTransaction.PosEntryMode)}, " +
                          $"PinEntryMode:{PaxPinEntryInfo.GetName(unacceptableTransaction.PinEntryInfo)}",
            ReturnStatus = ReturnStatus.NoAction,
            CreatedNameBy = unacceptableTransaction.Vendor,
            PfCommissionAmount = routeInfo.PfCommissionAmount,
            PfNetCommissionAmount = routeInfo.PfNetCommissionAmount,
            PfCommissionRate = routeInfo.PfCommissionRate,
            PfPerTransactionFee = routeInfo.PfPerTransactionFee,
            ParentMerchantCommissionAmount = routeInfo.ParentMerchantCommissionAmount,
            ParentMerchantCommissionRate = routeInfo.ParentMerchantCommissionRate,
            AmountWithoutCommissions = routeInfo.AmountWithoutCommissions,
            AmountWithoutBankCommission = routeInfo.AmountWithoutBankCommission,
            AmountWithoutParentMerchantCommission = routeInfo.AmountWithoutParentMerchantCommission,
            PricingProfileItemId = routeInfo.PricingProfileItem.Id,
            BsmvAmount = routeInfo.BsmvAmount,
            ProvisionNumber = unacceptableTransaction.ProvisionNo,
            PfPaymentDate = routeInfo.PfPaymentDate,
            BankPaymentDate = routeInfo.BankPaymentDate,
            PostingItemId = Guid.Empty,
            BlockageStatus = BlockageStatus.None,
            PfTransactionSource = PfTransactionSource.PhysicalPos,
            MerchantPhysicalPosId = merchantPhysicalPos.Id,
            PhysicalPosEodId = eod.Id,
            EndOfDayStatus = unacceptableTransaction.EndOfDayStatus,
            IsPerInstallment = routeInfo.ProfileSettlementMode == ProfileSettlementMode.PerInstallment ? true : false
        };
    }

    private static BankTransaction PopulateBankTransactionFromUnacceptable(
        PhysicalPosUnacceptableTransaction unacceptableTransaction,
        MerchantTransaction merchantTransaction, MerchantPhysicalPos merchantPhysicalPos)
    {
        return new BankTransaction
        {
            TransactionType = merchantTransaction.TransactionType,
            TransactionStatus = merchantTransaction.TransactionStatus,
            OrderId = merchantTransaction.OrderId,
            Amount = merchantTransaction.Amount,
            PointAmount = merchantTransaction.PointAmount,
            Currency = merchantTransaction.Currency,
            InstallmentCount = merchantTransaction.InstallmentCount,
            CardNumber = merchantTransaction.CardNumber,
            IsReverse = false,
            Is3ds = false,
            IssuerBankCode = merchantTransaction.IssuerBankCode,
            AcquireBankCode = merchantTransaction.AcquireBankCode,
            MerchantCode = merchantPhysicalPos.PhysicalPos.PfMainMerchantId,
            SubMerchantCode = unacceptableTransaction.MerchantId,
            BankOrderId = unacceptableTransaction.BankRef,
            RrnNumber = unacceptableTransaction.Rrn,
            ApprovalCode = unacceptableTransaction.ProvisionNo,
            Stan = unacceptableTransaction.Stan,
            BankResponseCode = unacceptableTransaction.AcquirerResponseCode,
            BankTransactionDate = merchantTransaction.TransactionDate,
            TransactionStartDate = merchantTransaction.TransactionStartDate,
            TransactionEndDate = merchantTransaction.TransactionEndDate,
            VposId = Guid.Empty,
            MerchantTransactionId = merchantTransaction.Id,
            MerchantPhysicalPosId = merchantTransaction.MerchantPhysicalPosId,
            EndOfDayStatus = unacceptableTransaction.EndOfDayStatus
        };
    }

    private async Task AuthRetryAsync(PhysicalPosUnacceptableTransaction unacceptableTransaction,
        MerchantPhysicalPos merchantPhysicalPos, Merchant merchant)
    {
        var routeInfo = await _posRouterService.PhysicalPosRouteAsync(
            merchant,
            unacceptableTransaction.BinNumber,
            merchantPhysicalPos.PhysicalPosId,
            unacceptableTransaction.Installment,
            unacceptableTransaction.Amount,
            unacceptableTransaction.PointAmount,
            unacceptableTransaction.Date,
            unacceptableTransaction.Currency,
            TransactionType.Auth);

        if (!routeInfo.IsSucceed)
        {
            throw new CustomApiException(routeInfo.ErrorCode, routeInfo.ErrorMessage);
        }

        var batchEod =
            await _dbContext.PhysicalPosEndOfDay.FirstOrDefaultAsync(s =>
                s.Id == unacceptableTransaction.PhysicalPosEodId);
        if (batchEod is null)
        {
            throw new NotFoundException(nameof(PhysicalPosEndOfDay), unacceptableTransaction.PhysicalPosEodId);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var merchantTransaction = await PopulateMerchantTransactionFromUnacceptableAsync(
                unacceptableTransaction, merchantPhysicalPos, merchant, batchEod, routeInfo, null);
            var bankTransaction =
                PopulateBankTransactionFromUnacceptable(unacceptableTransaction, merchantTransaction,
                    merchantPhysicalPos);
            bankTransaction.PhysicalPosEodId = batchEod.Id;

            unacceptableTransaction.CurrentStatus = UnacceptableTransactionStatus.Accepted;
            _dbContext.PhysicalPosUnacceptableTransaction.Update(unacceptableTransaction);

            await _dbContext.MerchantTransaction.AddAsync(merchantTransaction);
            await _dbContext.BankTransaction.AddAsync(bankTransaction);

            if (routeInfo.ProfileSettlementMode == ProfileSettlementMode.PerInstallment && merchantTransaction.TransactionStatus == TransactionStatus.Success)
            {
                await CalculateInstallmentTransactions(merchantTransaction, routeInfo, null, string.Empty, 0);
            }

            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });
    }

    private async Task ReturnRetryAsync(PhysicalPosUnacceptableTransaction unacceptableTransaction,
        MerchantPhysicalPos merchantPhysicalPos, Merchant merchant)
    {
        var referenceBankTransaction = await _dbContext.BankTransaction
            .FirstOrDefaultAsync(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.BankOrderId == unacceptableTransaction.OriginalRef &&
                s.SubMerchantCode == unacceptableTransaction.MerchantId &&
                s.AcquireBankCode == merchantPhysicalPos.PhysicalPos.AcquireBank.BankCode);

        if (referenceBankTransaction is null)
        {
            throw new InvalidReferenceNumberException();
        }

        var referenceMerchantTransaction = await _dbContext.MerchantTransaction
            .Include(s => s.AcquireBank)
            .FirstOrDefaultAsync(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.PfTransactionSource == PfTransactionSource.PhysicalPos &&
                (s.TransactionStatus != TransactionStatus.Fail && s.TransactionStatus != TransactionStatus.Returned) &&
                s.Id == referenceBankTransaction.MerchantTransactionId &&
                s.MerchantId == merchant.Id &&
                !s.IsReverse);
        if (referenceMerchantTransaction is null)
        {
            throw new InvalidReferenceNumberException();
        }

        if (referenceMerchantTransaction.IsChargeback || referenceMerchantTransaction.IsSuspecious)
        {
            throw new TransactionCanBeChargebackOrSuspiciousException();
        }

        var totalRefundAmount = await _dbContext.MerchantTransaction
            .Where(s =>
                s.ReturnedTransactionId == referenceMerchantTransaction.Id.ToString() &&
                s.MerchantId == merchant.Id &&
                s.TransactionStatus == TransactionStatus.Success)
            .SumAsync(s => s.Amount);

        if ((totalRefundAmount + unacceptableTransaction.Amount) > referenceBankTransaction.Amount)
        {
            throw new InvalidReturnAmountException();
        }

        var remainingReturnAmount =
            referenceBankTransaction.Amount - (totalRefundAmount + unacceptableTransaction.Amount);

        var routeInfo = await _posRouterService.PhysicalPosRouteAsync(
            merchant,
            unacceptableTransaction.BinNumber,
            merchantPhysicalPos.PhysicalPosId,
            unacceptableTransaction.Installment,
            unacceptableTransaction.Amount,
            unacceptableTransaction.PointAmount,
            unacceptableTransaction.Date,
            unacceptableTransaction.Currency,
            TransactionType.Return,
            referenceMerchantTransaction,
            remainingReturnAmount);

        if (!routeInfo.IsSucceed)
        {
            throw new CustomApiException(routeInfo.ErrorCode, routeInfo.ErrorMessage);
        }

        var batchEod =
            await _dbContext.PhysicalPosEndOfDay.FirstOrDefaultAsync(s =>
                s.Id == unacceptableTransaction.PhysicalPosEodId);
        if (batchEod is null)
        {
            throw new NotFoundException(nameof(PhysicalPosEndOfDay), unacceptableTransaction.PhysicalPosEodId);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var merchantTransaction = await PopulateMerchantTransactionFromUnacceptableAsync(
                unacceptableTransaction, merchantPhysicalPos, merchant, batchEod, routeInfo,
                referenceMerchantTransaction.ConversationId);
            merchantTransaction.ReturnedTransactionId = referenceMerchantTransaction.Id.ToString();

            var bankTransaction =
                PopulateBankTransactionFromUnacceptable(unacceptableTransaction, merchantTransaction,
                    merchantPhysicalPos);
            bankTransaction.PhysicalPosEodId = batchEod.Id;

            if (unacceptableTransaction.Status == PaxTransactionStatus.Approved)
            {
                var transactionStatus =
                    (totalRefundAmount + unacceptableTransaction.Amount) == referenceBankTransaction.Amount
                        ? TransactionStatus.Returned
                        : TransactionStatus.PartiallyReturned;

                referenceMerchantTransaction.ReturnAmount = totalRefundAmount + unacceptableTransaction.Amount;
                referenceMerchantTransaction.ReturnDate = unacceptableTransaction.Date;
                referenceMerchantTransaction.IsReturn = true;
                referenceMerchantTransaction.TransactionStatus = transactionStatus;
                referenceMerchantTransaction.LastModifiedBy = unacceptableTransaction.Vendor;
                referenceMerchantTransaction.ReturnStatus = ReturnStatus.Approved;
                _dbContext.Update(referenceMerchantTransaction);

                referenceBankTransaction.TransactionStatus = transactionStatus;
                referenceBankTransaction.LastModifiedBy = unacceptableTransaction.Vendor;
                _dbContext.Update(referenceBankTransaction);
            }

            unacceptableTransaction.CurrentStatus = UnacceptableTransactionStatus.Accepted;
            _dbContext.PhysicalPosUnacceptableTransaction.Update(unacceptableTransaction);

            await _dbContext.MerchantTransaction.AddAsync(merchantTransaction);
            await _dbContext.BankTransaction.AddAsync(bankTransaction);

            if (merchantTransaction.TransactionStatus == TransactionStatus.Success && merchantTransaction.IsPerInstallment == true)
            {
                await CalculateInstallmentTransactions(merchantTransaction, routeInfo, referenceMerchantTransaction.Id, unacceptableTransaction.Vendor, remainingReturnAmount);
            }

            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });
    }

    private async Task ReverseRetryAsync(PhysicalPosUnacceptableTransaction unacceptableTransaction,
        MerchantPhysicalPos merchantPhysicalPos,
        Merchant merchant)
    {
        var referenceBankTransaction = await _dbContext.BankTransaction
            .FirstOrDefaultAsync(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.BankOrderId == unacceptableTransaction.OriginalRef &&
                s.SubMerchantCode == unacceptableTransaction.MerchantId &&
                s.AcquireBankCode == merchantPhysicalPos.PhysicalPos.AcquireBank.BankCode);

        if (referenceBankTransaction is null)
        {
            throw new InvalidReferenceNumberException();
        }

        var referenceMerchantTransaction = await _dbContext.MerchantTransaction
            .Include(s => s.AcquireBank)
            .FirstOrDefaultAsync(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.PfTransactionSource == PfTransactionSource.PhysicalPos &&
                (s.TransactionStatus != TransactionStatus.Fail && s.TransactionStatus != TransactionStatus.Returned) &&
                s.Id == referenceBankTransaction.MerchantTransactionId &&
                s.MerchantId == merchant.Id &&
                !s.IsReverse);
        if (referenceMerchantTransaction is null)
        {
            throw new InvalidReferenceNumberException();
        }

        if (referenceMerchantTransaction.IsChargeback || referenceMerchantTransaction.IsSuspecious)
        {
            throw new TransactionCanBeChargebackOrSuspiciousException();
        }

        if (referenceMerchantTransaction.BatchStatus == BatchStatus.Completed)
        {
            throw new ReferenceTransactionPostingCompletedException();
        }

        var batchEod =
            await _dbContext.PhysicalPosEndOfDay.FirstOrDefaultAsync(s =>
                s.Id == unacceptableTransaction.PhysicalPosEodId);
        if (batchEod is null)
        {
            throw new NotFoundException(nameof(PhysicalPosEndOfDay), unacceptableTransaction.PhysicalPosEodId);
        }

        var bankTransaction = new BankTransaction
        {
            TransactionType = TransactionType.Reverse,
            TransactionStatus = unacceptableTransaction.Status switch
            {
                PaxTransactionStatus.Approved => TransactionStatus.Success,
                _ => TransactionStatus.Fail
            },
            OrderId = unacceptableTransaction.PaymentId,
            Amount = referenceBankTransaction.Amount,
            PointAmount = referenceBankTransaction.PointAmount,
            Currency = referenceBankTransaction.Currency,
            InstallmentCount = referenceBankTransaction.InstallmentCount,
            CardNumber = referenceBankTransaction.CardNumber,
            IsReverse = false,
            Is3ds = false,
            IssuerBankCode = referenceBankTransaction.IssuerBankCode,
            AcquireBankCode = referenceBankTransaction.AcquireBankCode,
            MerchantCode = referenceBankTransaction.MerchantCode,
            SubMerchantCode = referenceBankTransaction.SubMerchantCode,
            BankOrderId = unacceptableTransaction.BankRef,
            RrnNumber = unacceptableTransaction.Rrn,
            Stan = unacceptableTransaction.Stan,
            BankResponseCode = unacceptableTransaction.AcquirerResponseCode,
            BankTransactionDate = unacceptableTransaction.Date,
            TransactionStartDate = unacceptableTransaction.Date,
            TransactionEndDate = unacceptableTransaction.Date,
            VposId = Guid.Empty,
            MerchantTransactionId = referenceMerchantTransaction.Id,
            MerchantPhysicalPosId = referenceMerchantTransaction.MerchantPhysicalPosId,
            EndOfDayStatus = unacceptableTransaction.EndOfDayStatus,
            PhysicalPosEodId = batchEod.Id
        };

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            await _dbContext.AddAsync(bankTransaction);

            if (unacceptableTransaction.Status == PaxTransactionStatus.Approved)
            {
                referenceMerchantTransaction.IsReverse = true;
                referenceMerchantTransaction.ReverseDate = DateTime.Now;
                referenceMerchantTransaction.TransactionStatus = TransactionStatus.Reversed;
                referenceMerchantTransaction.LastModifiedBy = unacceptableTransaction.Vendor;
                referenceMerchantTransaction.PhysicalPosOldEodId = referenceMerchantTransaction.PhysicalPosEodId;
                referenceMerchantTransaction.PhysicalPosEodId = batchEod.Id;
                if (referenceMerchantTransaction.EndOfDayStatus == EndOfDayStatus.Suspended)
                {
                    referenceMerchantTransaction.BatchStatus =
                        unacceptableTransaction.EndOfDayStatus == EndOfDayStatus.Completed &&
                        referenceMerchantTransaction.BatchStatus == BatchStatus.EodPending
                            ? BatchStatus.Pending
                            : referenceMerchantTransaction.BatchStatus;
                }

                referenceMerchantTransaction.EndOfDayStatus = unacceptableTransaction.EndOfDayStatus;
                _dbContext.Update(referenceMerchantTransaction);

                referenceBankTransaction.IsReverse = true;
                referenceBankTransaction.ReverseDate = DateTime.Now;
                referenceBankTransaction.TransactionStatus = TransactionStatus.Reversed;
                referenceBankTransaction.LastModifiedBy = unacceptableTransaction.Vendor;
                referenceBankTransaction.EndOfDayStatus = unacceptableTransaction.EndOfDayStatus;
                _dbContext.Update(referenceBankTransaction);
            }

            unacceptableTransaction.CurrentStatus = UnacceptableTransactionStatus.Accepted;
            _dbContext.Update(unacceptableTransaction);

            if (referenceMerchantTransaction.TransactionType == TransactionType.Return)
            {
                var mainTransaction = await _dbContext.MerchantTransaction
                    .FirstOrDefaultAsync(s => s.Id == Guid.Parse(referenceMerchantTransaction.ReturnedTransactionId));

                var successfullyReturnedTransactions = _dbContext.MerchantTransaction
                    .Where(s => s.TransactionType == TransactionType.Return &&
                                s.TransactionStatus == TransactionStatus.Success &&
                                s.ReturnedTransactionId == mainTransaction.Id.ToString());

                var countOfReturnedTransactions = successfullyReturnedTransactions.Count();
                var sumOfReturnedTransactionAmounts = successfullyReturnedTransactions.Sum(s => s.Amount);

                var lastReturnTransaction = await successfullyReturnedTransactions
                    .Where(s => s.Id != referenceMerchantTransaction.Id)
                    .OrderByDescending(s => s.CreateDate)
                    .FirstOrDefaultAsync();

                var sumOfReversedReturnTransactions = _dbContext.MerchantTransaction
                    .Where(s =>
                        s.TransactionType == TransactionType.Return &&
                        s.TransactionStatus == TransactionStatus.Reversed &&
                        s.ReturnedTransactionId == mainTransaction.Id.ToString())
                    .Sum(s => s.Amount);

                var allAmountReversed = mainTransaction.Amount ==
                                        (referenceMerchantTransaction.Amount + sumOfReversedReturnTransactions);

                if (allAmountReversed || countOfReturnedTransactions <= 1)
                {
                    mainTransaction.TransactionStatus = TransactionStatus.Success;
                    mainTransaction.ReturnAmount = 0;
                    mainTransaction.IsReturn = false;
                }
                else
                {
                    mainTransaction.ReturnAmount =
                        sumOfReturnedTransactionAmounts - referenceMerchantTransaction.Amount;
                    mainTransaction.TransactionStatus = TransactionStatus.PartiallyReturned;
                    mainTransaction.IsReturn = true;
                }

                mainTransaction.ReturnDate = (allAmountReversed || lastReturnTransaction is null)
                    ? DateTime.MinValue
                    : lastReturnTransaction.CreateDate;

                mainTransaction.BankCommissionAmount = allAmountReversed
                    ? (mainTransaction.BankCommissionRate / 100m) * mainTransaction.Amount
                    : (mainTransaction.BankCommissionRate / 100m) *
                      (mainTransaction.Amount - mainTransaction.ReturnAmount);

                _dbContext.Update(mainTransaction);
            }

            if (unacceptableTransaction.Status == PaxTransactionStatus.Approved && referenceMerchantTransaction.IsPerInstallment == true)
            {
                var installmentTransactions = await _merchantInstallmentTransactionRepository.GetAll().Where(b => b.MerchantTransactionId == referenceMerchantTransaction.Id).ToListAsync();

                //if return

                if (installmentTransactions.Any())
                {
                    foreach (var item in installmentTransactions)
                    {
                        item.IsReverse = true;
                        item.ReverseDate = DateTime.Now;
                        item.TransactionStatus = TransactionStatus.Reversed;
                        item.LastModifiedBy = unacceptableTransaction.Vendor;
                        item.PhysicalPosOldEodId = referenceMerchantTransaction.PhysicalPosOldEodId;
                        item.PhysicalPosEodId = referenceMerchantTransaction.PhysicalPosEodId;
                    }

                    _dbContext.UpdateRange(installmentTransactions);
                }
            }

            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });
    }
}