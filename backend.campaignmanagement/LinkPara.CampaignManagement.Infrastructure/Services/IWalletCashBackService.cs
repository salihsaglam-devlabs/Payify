using LinkPara.CampaignManagement.Application.Commons.Exceptions;
using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Models;
using LinkPara.CampaignManagement.Application.Features.IWalletCashbacks.Commands.CashBack;
using LinkPara.CampaignManagement.Domain.Entities;
using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class IWalletCashbackService : IIWalletCashbackService
{

    private readonly IApplicationUserService _applicationUserService;
    private readonly IGenericRepository<IWalletCashbackTransaction> _repository;
    private readonly IGenericRepository<IWalletCharge> _chargeRepository;
    private readonly IGenericRepository<IWalletChargeTransaction> _chargeTransactionRepository;
    private readonly ILogger<IWalletCashbackService> _logger;
    private readonly IEmoneyService _eMoneyService;
    private readonly IConfiguration _configuration;
    private readonly IParameterService _parameterService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAccountingService _accountingService;

    public IWalletCashbackService(IApplicationUserService applicationUserService,
        IGenericRepository<IWalletCashbackTransaction> repository,
        IGenericRepository<IWalletCharge> chargeRepository,
        ILogger<IWalletCashbackService> logger,
        IEmoneyService eMoneyService,
        IConfiguration configuration,
        IGenericRepository<IWalletChargeTransaction> chargeTransactionRepository,
        IParameterService parameterService,
        IHttpContextAccessor httpContextAccessor,
        IAccountingService accountingService)
    {
        _applicationUserService = applicationUserService;
        _repository = repository;
        _chargeRepository = chargeRepository;
        _logger = logger;
        _eMoneyService = eMoneyService;
        _configuration = configuration;
        _chargeTransactionRepository = chargeTransactionRepository;
        _parameterService = parameterService;
        _httpContextAccessor = httpContextAccessor;
        _accountingService = accountingService;
    }

    public async Task SaveCashBackTransactionAsync(CashBackCommand request)
    {
        var chargeGuid = request.sales_transactions?.FirstOrDefault().process_guid;

        if (chargeGuid is null)
        {
            throw new NotFoundException(nameof(request.sales_transactions));
        }

        var charge = await _chargeRepository.GetAll().Where(x => x.Id == chargeGuid).SingleOrDefaultAsync();

        if (charge is null)
        {
            throw new NotFoundException(nameof(charge), chargeGuid);
        }

        await SaveCashbackChargeTransactionAsync(request, charge);

        UpdateChargeMerchantInformation(charge, request);

        var currencyCode = await GetCurrencyCodeAsync(charge);

        var orderId = request.sales_transactions.FirstOrDefault().oid;

        var provision = await CreateProvisionAsync(charge, currencyCode, orderId);
        await SaveSalesChargeTransactionAsync(charge, request, provision,orderId);

        await _accountingService.PostAccountingPaymentAsync(new AccountingPayment
        {
            AccountingTransactionType = AccountingTransactionType.Emoney,
            AccountingCustomerType = AccountingCustomerType.Emoney,
            Amount = charge.Amount,
            CurrencyCode = currencyCode,
            OperationType = OperationType.CampaignPayment,
            Source = $"WA-{charge.WalletNumber}",
            TransactionDate = DateTime.Now,
            UserId = charge.UserId
        });

        await CashbackAsync(request, provision, charge);

        await _accountingService.PostAccountingPaymentAsync(new AccountingPayment
        {
            AccountingTransactionType = AccountingTransactionType.Emoney,
            AccountingCustomerType = AccountingCustomerType.Emoney,
            Amount = request.reward_transactions.amount,
            CurrencyCode = currencyCode,
            OperationType = OperationType.CampaignCashback,
            Destination = $"WA-{charge.WalletNumber}",
            TransactionDate = DateTime.Now,
            UserId = charge.UserId
        });

        var isSaveSalesTransactions = _configuration.GetValue<bool>("IsSaveSalesTransactions", false);
        if (isSaveSalesTransactions)
        {

            List<IWalletCashbackTransaction> salesTransactions = MapSalesTransactions(request.sales_transactions, request.hash_data, charge.Id);

            foreach (var salesTransaction in salesTransactions)
            {
                await _repository.AddAsync(salesTransaction);
            }

            IWalletCashbackTransaction rewardTransaction = MapCashbackTransaction(request.reward_transactions, request.hash_data, charge.Id);

            await _repository.AddAsync(rewardTransaction);
        }
    }

    private void UpdateChargeMerchantInformation(IWalletCharge charge, CashBackCommand request)
    {
        charge.MerchantBranchId = request.sales_transactions.FirstOrDefault().merchant_branch_id;
        charge.MerchantName = request.merchant_name;
        charge.MerchantId = request.sales_transactions.FirstOrDefault().merchant_id;
        charge.MerchantBranchName = request.sales_transactions.FirstOrDefault().merchant_branch_name;
    }

    private async Task CashbackAsync(CashBackCommand request, ProvisionResponse provision, IWalletCharge charge)
    {
        var provisionCashbackRequest = new ProvisionCashbackRequest
        {
            Amount = request.reward_transactions.amount,
            ProvisionReference = provision.ReferenceNumber,
            UserId = charge.UserId,
            WalletNumber = charge.WalletNumber,
        };

        await _eMoneyService.ProvisionCashbackAsync(provisionCashbackRequest);
    }

    private async Task SaveCashbackChargeTransactionAsync(CashBackCommand request, IWalletCharge charge)
    {
        await _chargeTransactionRepository.AddAsync(
            new IWalletChargeTransaction
            {
                Amount = request.reward_transactions.amount,
                ChargeTransactionType = ChargeTransactionType.Cashback,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                IWalletChargeId = charge.Id
            }
        );
    }

    private async Task SaveSalesChargeTransactionAsync(IWalletCharge charge, CashBackCommand request, ProvisionResponse provision, int orderId)
    {
        var totalSalesAmount = request.sales_transactions.Sum(x => x.amount);

        await _chargeTransactionRepository.AddAsync(
            new IWalletChargeTransaction
            {
                Amount = totalSalesAmount,
                ChargeTransactionType = ChargeTransactionType.Sales,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                IWalletChargeId = charge.Id,
                ProvisionReferenceNumber = provision.ReferenceNumber,
                ProvisionConversationId = provision.ConversationId,
                OrderId = orderId
            }
        );
    }

    private async Task<string> GetCurrencyCodeAsync(IWalletCharge charge)
    {
        var parametersTemplateValues = await _parameterService.GetAllParameterTemplateValuesAsync("Currencies", "");

        var currencyParameter = parametersTemplateValues.
            FirstOrDefault(x => x.TemplateValue == charge.CurrencyCode.ToString()
                             && x.TemplateCode == "Number");
        return currencyParameter is null ? "TRY" : currencyParameter.ParameterCode;
    }

    private async Task<ProvisionResponse> CreateProvisionAsync(IWalletCharge charge, string currencyCode, int orderId)
    {
        var clientIpAddress = _httpContextAccessor.HttpContext?.Request?.Headers["ClientIpAddress"];

        var provisionRequest = new ProvisionRequest
        {
            Amount = charge.Amount,
            ClientIpAddress = clientIpAddress ?? "IpNotFound",
            ConversationId = Guid.NewGuid().ToString(),
            ProvisionSource = ProvisionSource.IWallet,
            UserId = charge.UserId,
            WalletNumber = charge.WalletNumber,
            CurrencyCode = currencyCode,
            Tag = $"{charge.MerchantName} - {orderId}",
            Description = charge.MerchantName,
        };

        ProvisionResponse provision;

        try
        {
            provision = await _eMoneyService.CreateProvisionAsync(provisionRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError("ProvisionError : {Exception}", exception);
            charge.ChargeStatus = ChargeStatus.Error;
            charge.ExceptionMessage = "ProvisionError";
            await _chargeRepository.UpdateAsync(charge);

            throw;
        }

        if (!provision.IsSucceed)
        {
            _logger.LogError("ProvisionUnsuccess Message: {ErrorMessage}", provision.ErrorMessage);
            charge.ChargeStatus = ChargeStatus.Error;
            charge.ExceptionMessage = $"ProvisionUnsuccess:{provision.ErrorMessage}";
            await _chargeRepository.UpdateAsync(charge);

            throw new ProvisionErrorException();
        }

        charge.ChargeStatus = ChargeStatus.Finished;
        await _chargeRepository.UpdateAsync(charge);

        return provision;
    }

    private IWalletCashbackTransaction MapCashbackTransaction(CashbackTransaction reward_transactions, string hashData, Guid chargeId)
    {
        DateTime createdAt = DateTime.MinValue;
        if (DateTime.TryParse(reward_transactions.created_at, out DateTime result))
        {
            createdAt = result;
        }

        var rewardTransaction = new IWalletCashbackTransaction
        {
            Amount = reward_transactions.amount,
            Balance = reward_transactions.balance,
            CommissionAmount = reward_transactions.comsn_amount,
            CommissionRate = reward_transactions.comsn_rate,
            CreatedAt = createdAt,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            CustomerBranchId = reward_transactions.customer_branch_id,
            CustomerId = reward_transactions.customer_id,
            ExternalOrderId = reward_transactions.order_id,
            ExternalStatus = reward_transactions.status,
            Oid = reward_transactions.oid,
            IWalletCardId = reward_transactions.card_id,
            HashData = hashData,
            IWalletChargeId = chargeId,
            LoadType = reward_transactions.load_type,
            MerchantBranchId = reward_transactions.merchant_branch_id,
            MerchantBranchName = reward_transactions.merchant_branch_name,
            MerchantId = reward_transactions.merchant_id,
            MerchantName = reward_transactions.merchant_name,
            PosId = reward_transactions.pos_id,
            QrCode = reward_transactions.qr_code,
            TransactionType = IWalletTransactionType.Cashback,
            VatRate = reward_transactions.vat_rate,
            WalletId = reward_transactions.wallet_id,
            WalletNumber = reward_transactions.ext_wallet_id
        };
        return rewardTransaction;
    }

    private List<IWalletCashbackTransaction> MapSalesTransactions(List<CashbackTransaction> sales_transactions, string hashData, Guid chargeId)
    {
        var salesTransactions = new List<IWalletCashbackTransaction>();
        foreach (var s in sales_transactions)
        {
            DateTime createdAt = DateTime.MinValue;
            if (DateTime.TryParse(s.created_at, out DateTime result))
            {
                createdAt = result;
            }
            var salesTransaction = new IWalletCashbackTransaction
            {
                Amount = s.amount,
                Balance = s.balance,
                CommissionAmount = s.comsn_amount,
                CommissionRate = s.comsn_rate,
                CreatedAt = createdAt,
                CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                CustomerBranchId = s.customer_branch_id,
                CustomerId = s.customer_id,
                ExternalOrderId = s.order_id,
                ExternalStatus = s.status,
                Oid = s.oid,
                IWalletCardId = s.card_id,
                HashData = hashData,
                IWalletChargeId = chargeId,
                LoadType = s.load_type,
                MerchantBranchId = s.merchant_branch_id,
                MerchantBranchName = s.merchant_branch_name,
                MerchantId = s.merchant_id,
                MerchantName = s.merchant_name,
                PosId = s.pos_id,
                QrCode = s.qr_code,
                TransactionType = IWalletTransactionType.Sales,
                VatRate = s.vat_rate,
                WalletId = s.wallet_id,
                WalletNumber = s.ext_wallet_id
            };

            salesTransactions.Add(salesTransaction);
        }
        return salesTransactions;
    }
}
