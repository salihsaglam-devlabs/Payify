using System.Net.Http.Json;
using AutoMapper;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.Payments;
using LinkPara.PF.Application.Commons.Models.VposModels.VposShortCircuit;
using LinkPara.PF.Application.Features.CardBins;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class PosRouterService : IPosRouterService
{
    private readonly ILogger<PosRouterService> _logger;
    private readonly IGenericRepository<VposBankApiInfo> _vposBankInfoRepository;
    private readonly IGenericRepository<CostProfileItem> _costProfileItemRepository;
    private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
    private readonly IGenericRepository<BankLimit> _bankLimitRepository;
    private readonly IGenericRepository<BankHealthCheck> _bankHealthCheckRepository;
    private readonly PfDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IVaultClient _vaultClient;
    private readonly HttpClient _httpClient;
    private readonly ICardBinService _binService;
    private readonly IParameterService _parameterService;
    private readonly IBasePaymentService _basePaymentService;

    private const int InstallmentTwentyFour = 24;
    private const int InstallmentThirtySix = 36;

    public PosRouterService(IGenericRepository<VposBankApiInfo> vposBankInfoRepository,
        IGenericRepository<CostProfileItem> costProfileItemRepository,
        IGenericRepository<MerchantVpos> merchantVposRepository,
        PfDbContext dbContext,
        IGenericRepository<BankLimit> bankLimitRepository,
        IGenericRepository<BankHealthCheck> bankHealthCheckRepository,
        ILogger<PosRouterService> logger,
        IMapper mapper,
        IVaultClient vaultClient,
        HttpClient httpClient, ICardBinService binService, IParameterService parameterService, IBasePaymentService basePaymentService)
    {
        _vposBankInfoRepository = vposBankInfoRepository;
        _costProfileItemRepository = costProfileItemRepository;
        _merchantVposRepository = merchantVposRepository;
        _dbContext = dbContext;
        _bankLimitRepository = bankLimitRepository;
        _bankHealthCheckRepository = bankHealthCheckRepository;
        _logger = logger;
        _mapper = mapper;
        _vaultClient = vaultClient;
        _httpClient = httpClient;
        _binService = binService;
        _parameterService = parameterService;
        _basePaymentService = basePaymentService;
    }

    public async Task<RouteResponse> RouteAsync(CardBinDto bin, MerchantDto merchant, string currency, int installment, decimal amount, Guid selectedVposId, bool? isInsuranceVpos = false, bool? isTopupVpos = false)
    {
        var merchantVpos = new List<MerchantVpos>();

        if (bin is null)
        {
            if (installment > 1)
            {
                throw new InternationalCardInstallmentNotAllowedException();
            }

            var merchantVposQuery = _merchantVposRepository.GetAll()
                       .Include(s => s.Vpos)
                       .Include(s => s.Vpos.CostProfiles)
                       .Include(b => b.Merchant)
                       .Where(s =>
                           s.RecordStatus == RecordStatus.Active &&
                           s.TerminalStatus == TerminalStatus.Active &&
                           s.MerchantId == merchant.Id &&
                           s.Vpos.VposStatus == VposStatus.Active &&
                           s.Vpos.SecurityType != SecurityType.Unknown &&
                           s.Vpos.IsInsuranceVpos == isInsuranceVpos &&
                           s.Vpos.IsOnUsVpos == false &&
                           s.Merchant.InternationalCardAllowed).AsQueryable();

            if (selectedVposId != Guid.Empty)
            {
                merchantVposQuery = merchantVposQuery.Where(s => s.VposId == selectedVposId);
            }

            if (isTopupVpos == true)
            {
                merchantVposQuery = merchantVposQuery.Where(b => b.Vpos.IsTopUpVpos == true);
            }

            merchantVpos = await merchantVposQuery.ToListAsync();

            if (!merchantVpos.Any())
            {
                throw new VposNotFoundException();
            }

            var routerMerchantVposDto = _mapper.Map<List<MerchantVpos>, List<RouterMerchantVposDto>>(merchantVpos);

            var checkMerchantVpos = await CheckMerchantVposAsync(routerMerchantVposDto, installment, amount, BankLimitType.International);

            checkMerchantVpos = ValidateMerchantVpos(checkMerchantVpos);

            var pricing = await VposPricing(checkMerchantVpos, merchant, bin, currency, amount, CardTransactionType.NotOnUs);

            if (!pricing.IsSucceed)
            {
                throw new PricingProfileItemNotFoundException();
            }

            return pricing;
        }
        else
        {
            var merchantVposQuery = _merchantVposRepository.GetAll()
                      .Include(s => s.Vpos)
                      .Include(s => s.Vpos.CostProfiles)
                      .Include(s => s.Vpos.AcquireBank)
                      .Include(s => s.Merchant)
                      .Where(s =>
                          s.RecordStatus == RecordStatus.Active &&
                          s.TerminalStatus == TerminalStatus.Active &&
                          s.MerchantId == merchant.Id &&
                          s.Vpos.IsInsuranceVpos == isInsuranceVpos &&
                          s.Vpos.IsOnUsVpos == false &&
                          s.Vpos.SecurityType != SecurityType.Unknown &&
                          s.Vpos.VposStatus == VposStatus.Active)
                      .AsQueryable();

            if (selectedVposId != Guid.Empty)
            {
                merchantVposQuery = merchantVposQuery.Where(s => s.VposId == selectedVposId);
            }

            if (isTopupVpos == true)
            {
                merchantVposQuery = merchantVposQuery.Where(b => b.Vpos.IsTopUpVpos == true);
            }

            merchantVpos = await merchantVposQuery.ToListAsync();

            if (!merchantVpos.Any())
            {
                throw new VposNotFoundException();
            }

            var routerMerchantVposDto = _mapper.Map<List<MerchantVpos>, List<RouterMerchantVposDto>>(merchantVpos);

            var onUsPricing = await CheckRestrictOwnCardNotOnUs(routerMerchantVposDto, merchant, currency, installment, amount, bin);

            if (onUsPricing.IsSucceed)
            {
                return onUsPricing;
            }

            var loyalty = GetCardLoyalties(bin, installment);

            var onUsVpos = await OnUsPricingAsync(routerMerchantVposDto, merchant, loyalty, bin, installment, amount);

            onUsVpos = ValidateMerchantVpos(onUsVpos);

            onUsPricing = await VposPricing(onUsVpos, merchant, bin, currency, amount, CardTransactionType.OnUs, installment);

            if (onUsPricing.IsSucceed && onUsPricing.AcquireBank.RestrictOwnCardNotOnUs)
            {
                return onUsPricing;
            }

            var notOnUsVpos = await NotOnUsPricingAsync(routerMerchantVposDto, merchant, loyalty, bin, installment, amount);

            notOnUsVpos = ValidateMerchantVpos(notOnUsVpos);

            if (notOnUsVpos.Any())
            {
                var notOnUsPricing = await VposPricing(notOnUsVpos, merchant, bin, currency, amount, CardTransactionType.NotOnUs, installment);

                if (notOnUsPricing.IsSucceed)
                {
                    if (onUsPricing.IsSucceed == false)
                    {
                        return notOnUsPricing;
                    }
                    else if (onUsPricing.IsOptionalPos == true && notOnUsPricing.IsOptionalPos == false)
                    {
                        return notOnUsPricing;
                    }
                    else if (onUsPricing.IsOptionalPos == false && notOnUsPricing.IsOptionalPos == true)
                    {
                        return onUsPricing;
                    }
                    else if (onUsPricing.IsHealthCheckOptionalPos == true && notOnUsPricing.IsHealthCheckOptionalPos == false)
                    {
                        return notOnUsPricing;
                    }
                    else if (onUsPricing.IsHealthCheckOptionalPos == false && notOnUsPricing.IsHealthCheckOptionalPos == true)
                    {
                        return onUsPricing;
                    }
                    else if (notOnUsPricing.CommissionAmount < onUsPricing.CommissionAmount || !onUsVpos.Any())
                    {
                        return notOnUsPricing;
                    }
                    else if (notOnUsPricing.CommissionAmount == onUsPricing.CommissionAmount)
                    {
                        if (notOnUsPricing.BlockedDayNumber < onUsPricing.BlockedDayNumber)
                        {
                            return notOnUsPricing;
                        }
                        else if (notOnUsPricing.BlockedDayNumber == onUsPricing.BlockedDayNumber)
                        {
                            if (onUsPricing is not null)
                            {
                                var cardBankCodeExistence = CheckCardBankCodeExistence(routerMerchantVposDto, merchant, bin);
                                var cardBankPricing = await VposPricing(cardBankCodeExistence, merchant, bin, currency, amount, CardTransactionType.OnUs);

                                if (cardBankPricing.IsSucceed)
                                {
                                    return cardBankPricing;
                                }
                                else
                                {
                                    return onUsPricing;
                                }
                            }
                            else
                            {
                                return notOnUsPricing;
                            }
                        }
                    }
                }
            }

            if (!onUsPricing.IsSucceed)
            {
                throw new VposNotFoundException();
            }

            return onUsPricing;
        }
    }
    private List<RouterMerchantVposDto> ValidateMerchantVpos(List<RouterMerchantVposDto> merchantVposList)
    {
        if (merchantVposList.Where(b => b.IsOptionalPos == false && b.IsHealthCheckOptionalPos == false).Any())
        {
            merchantVposList = merchantVposList.Where(b => b.IsOptionalPos == false && b.IsHealthCheckOptionalPos == false).ToList();
        }
        else
        {
            if (merchantVposList.Where(b => b.IsOptionalPos == false).Any())
            {
                merchantVposList = merchantVposList.Where(b => b.IsOptionalPos == false).ToList();
            }
            else if (merchantVposList.Where(b => b.IsHealthCheckOptionalPos == false).Any())
            {
                merchantVposList = merchantVposList.Where(b => b.IsHealthCheckOptionalPos == false).ToList();
            }
        }

        return merchantVposList;
    }
    public async Task<RouteResponse> OnUsRouteAsync(MerchantDto merchant, string currency, decimal amount)
    {
        var merchantVpos = await _merchantVposRepository.GetAll()
            .Include(s => s.Vpos)
            .Include(s => s.Vpos.AcquireBank)
            .Include(b => b.Merchant)
            .Where(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.MerchantId == merchant.Id &&
                s.Vpos.VposStatus == VposStatus.Active &&
                s.Vpos.IsOnUsVpos)
            .FirstOrDefaultAsync();

        if (merchantVpos is null)
        {
            throw new VposNotFoundException();
        }

        return new RouteResponse
        {
            AcquireBank = merchantVpos.Vpos.AcquireBank,
            Vpos = merchantVpos.Vpos,
            CardTransactionType = CardTransactionType.OnUs,
            CommissionRate = 0,
            CommissionAmount = 0,
            BlockedDayNumber = 0,
            IsSucceed = true
        };
    }

    public async Task<Guid> CheckRouteForShortCircuitAsync(Guid merchantId, string cardNumber, string currency,
        int installment, decimal amount, bool? isInsuranceVpos = false)
    {
        try
        {
            var vposRouteShortCircuit = await _vaultClient.GetSecretValueAsync<VposRouteShortCircuitModel>("PFSecrets", "VposRouteShortCircuit");

            if (!vposRouteShortCircuit.IsEnabled)
            {
                return Guid.Empty;
            }

            _httpClient.BaseAddress = new Uri(vposRouteShortCircuit.RequestEndpoint);
            var request = new HttpRequestMessage(HttpMethod.Post, "")
            {
                Content = JsonContent.Create(new ShortCircuitRequestModel
                {
                    MerchantId = merchantId.ToString(),
                    CardNumber = cardNumber,
                    InstallmentCount = installment,
                    RulesetId = vposRouteShortCircuit.RulesetId,
                    Amount = amount,
                    Currency = currency
                })
            };

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return Guid.Empty;
            }

            var result = await response.Content.ReadFromJsonAsync<ShortCircuitResponseModel>();
            if (!result.Success)
            {
                return Guid.Empty;
            }

            var selectedVposId = Guid.Parse(result.SelectedVposId);
            var isMerchantVposExists = await _merchantVposRepository.GetAll()
                .Include(s => s.Vpos)
                .Where(s =>
                    s.RecordStatus == RecordStatus.Active &&
                    s.TerminalStatus == TerminalStatus.Active &&
                    s.MerchantId == merchantId &&
                    s.Vpos.VposStatus == VposStatus.Active &&
                    s.Vpos.IsInsuranceVpos == isInsuranceVpos &&
                    s.VposId == selectedVposId)
                .AnyAsync();

            return isMerchantVposExists ? selectedVposId : Guid.Empty;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error occured on vpos short circuit control:{exception}");
            return Guid.Empty;
        }
    }
    public async Task<PhysicalPosRouteResponse> PhysicalPosRouteAsync(Merchant merchant, string bin, Guid physicalPosId, int installment, decimal amount, decimal pointAmount, DateTime transactionDate, string currency, TransactionType transactionType, MerchantTransaction referenceMerchantTransaction = null, decimal remainingReturnAmount = 0)
    {
        var physicalPos = await _dbContext.PhysicalPos.Include(s => s.AcquireBank).FirstOrDefaultAsync(s => s.Id == physicalPosId);
        if (physicalPos is null)
        {
            return new PhysicalPosRouteResponse
            {
                IsSucceed = false,
                ErrorCode = ApiErrorCode.PhysicalPosNotFound
            };
        }

        return transactionType switch
        {
            TransactionType.Auth => await PhysicalPosAuthRouteAsync(merchant, bin, physicalPos, installment, amount, pointAmount, transactionDate, currency),
            TransactionType.Return => await PhysicalPosReturnRouteAsync(bin, physicalPos, installment, amount, pointAmount, transactionDate, referenceMerchantTransaction, remainingReturnAmount),
            _ => new PhysicalPosRouteResponse { IsSucceed = false, ErrorCode = ApiErrorCode.InvalidTransactionType }
        };
    }

    private async Task<PhysicalPosRouteResponse> PhysicalPosReturnRouteAsync(string binNumber,
        Domain.Entities.PhysicalPos.PhysicalPos physicalPos, int installment, decimal amount, decimal pointAmount, DateTime transactionDate,
        MerchantTransaction referenceMerchantTransaction, decimal remainingReturnAmount = 0)
    {
        var bin = await _binService.GetByNumberAsync(binNumber);
        var pricingProfileItem = await _dbContext.PricingProfileItem
            .Include(b=>b.PricingProfileInstallments)
            .Include(pricingProfileItem => pricingProfileItem.PricingProfile)
            .ThenInclude(pricingProfile => pricingProfile.Currency)
            .FirstOrDefaultAsync(p => p.Id == referenceMerchantTransaction.PricingProfileItemId);

        var perTransactionFee = remainingReturnAmount == 0 ? pricingProfileItem.PricingProfile.PerTransactionFee : 0;
        var bankCommissionAmount = (referenceMerchantTransaction.BankCommissionRate / 100m) * amount;
        var pfCommissionAmount = perTransactionFee + pricingProfileItem.CommissionRate / 100m * amount;
        var parentMerchantCommissionAmount =
            pricingProfileItem.ParentMerchantCommissionRate / 100m * amount;

        var pfPaymentDate = transactionDate;
        if (referenceMerchantTransaction.PfPaymentDate <= DateTime.Now)
        {
            pfPaymentDate = DateTime.Now.AddDays(1);
        }

        return new PhysicalPosRouteResponse
        {
            IsSucceed = true,
            CardType = referenceMerchantTransaction.CardType,
            CardTransactionType = referenceMerchantTransaction.CardTransactionType ?? CardTransactionType.NotOnUs,
            AcquireBank = physicalPos.AcquireBank,
            PricingProfileItem = pricingProfileItem,
            IsAmex = referenceMerchantTransaction.IsAmex,
            IsInternational = referenceMerchantTransaction.IsInternational,
            InstallmentCount = installment,
            BinNumber = binNumber,
            IssuerBankCode = bin?.BankCode ?? 0,
            CurrencyNumber = pricingProfileItem.PricingProfile.Currency.Number,
            Amount = amount,
            PointAmount = pointAmount,
            PointCommissionRate = referenceMerchantTransaction.PointCommissionRate,//diğer yerlerde onus ise al değilse 0 hesapla yapılmış
            PointCommissionAmount = (pointAmount * referenceMerchantTransaction.PointCommissionRate) / 100m,//paymentService içerisinde tüm amount üzerinden hesaplanmış
            BankCommissionRate = referenceMerchantTransaction.BankCommissionRate,
            BankCommissionAmount = bankCommissionAmount,
            PfCommissionRate = pricingProfileItem.CommissionRate,
            PfCommissionAmount = pfCommissionAmount,
            PfNetCommissionAmount = pfCommissionAmount - bankCommissionAmount,
            PfPerTransactionFee = perTransactionFee,
            ParentMerchantCommissionRate = pricingProfileItem.ParentMerchantCommissionRate,
            ParentMerchantCommissionAmount = parentMerchantCommissionAmount,
            ServiceCommissionRate = referenceMerchantTransaction.ServiceCommissionRate,//diğer yerlerde onus ise al değilse 0 hesapla yapılmış
            ServiceCommissionAmount = (amount * referenceMerchantTransaction.ServiceCommissionRate) / 100m,
            AmountWithoutCommissions = amount - pfCommissionAmount - parentMerchantCommissionAmount,
            AmountWithoutBankCommission = amount - bankCommissionAmount,
            AmountWithoutParentMerchantCommission = amount - parentMerchantCommissionAmount,
            BsmvAmount = await BsmvAmountCalculateHelper.CalculateBsmvAmount(pfCommissionAmount - bankCommissionAmount, _parameterService),
            BankPaymentDate = transactionDate.Date.AddDays(1),
            PfPaymentDate = pfPaymentDate
        };
    }

    private async Task<PhysicalPosRouteResponse> PhysicalPosAuthRouteAsync(Merchant merchant, string binNumber, Domain.Entities.PhysicalPos.PhysicalPos physicalPos, int installment, decimal amount, decimal pointAmount, DateTime transactionDate, string currency)
    {
        var bin = await _binService.GetByNumberAsync(binNumber);
        var loyaltyExceptions = bin is not null ? await _dbContext.CardLoyaltyException
            .Where(s => s.CounterBankCode == bin.BankCode && s.RecordStatus == RecordStatus.Active)
            .ToListAsync() : [];
        var cardTransactionType = CardHelper.GetCardTransactionType(physicalPos.AcquireBank, bin, loyaltyExceptions);
        var cardType = GetCardType(bin);

        var costProfileItemQuery = _costProfileItemRepository.GetAll()
            .Include(s => s.CostProfile).Include(b => b.CostProfileInstallments)
            .Where(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.CostProfile.ProfileStatus == ProfileStatus.InUse &&
                s.IsActive &&
                s.CostProfile.CurrencyCode == currency &&
                s.CostProfile.PosType == PosType.Physical &&
                s.ProfileCardType == cardType &&
                s.CardTransactionType == cardTransactionType &&
                s.CostProfile.PhysicalPosId == physicalPos.Id);

        if (installment <= 12)
        {
            costProfileItemQuery = costProfileItemQuery.Where(s => s.InstallmentNumber == installment);
        }
        else
        {
            var endInstallmentNumber = (installment <= 24) ? InstallmentTwentyFour : InstallmentThirtySix;
            costProfileItemQuery = costProfileItemQuery.Where(s => s.InstallmentNumberEnd == endInstallmentNumber);
        }
        var costProfileItem = await costProfileItemQuery.FirstOrDefaultAsync();
        if (costProfileItem is null)
        {
            return new PhysicalPosRouteResponse
            {
                IsSucceed = false,
                ErrorCode = ApiErrorCode.CostProfileNotFound
            };
        }

        var pricingProfileItemQuery = _dbContext.PricingProfileItem
            .Include(b=>b.PricingProfileInstallments)
            .Include(p => p.PricingProfile)
            .ThenInclude(s => s.Currency)
            .Where(w =>
                w.PricingProfile.PricingProfileNumber == merchant.PricingProfileNumber &&
                w.PricingProfile.ProfileStatus == ProfileStatus.InUse &&
                w.PricingProfile.CurrencyCode == currency &&
                w.ProfileCardType == cardType &&
                w.RecordStatus == RecordStatus.Active &&
                w.IsActive);

        if (installment <= 12)
        {
            pricingProfileItemQuery = pricingProfileItemQuery.Where(s => s.InstallmentNumber == installment);
        }
        else
        {
            var endInstallmentNumber = installment <= 24 ? InstallmentTwentyFour : InstallmentThirtySix;
            pricingProfileItemQuery = pricingProfileItemQuery.Where(s => s.InstallmentNumberEnd == endInstallmentNumber);
        }
        var pricingProfileItem = await pricingProfileItemQuery.FirstOrDefaultAsync();
        if (pricingProfileItem is null)
        {
            return new PhysicalPosRouteResponse
            {
                IsSucceed = false,
                ErrorCode = ApiErrorCode.InvalidMerchantPricingProfile
            };
        }

        var totalBankCommissionRate = costProfileItem.CommissionRate +
                                      costProfileItem.CostProfile.ServiceCommission +
                                      (cardTransactionType == CardTransactionType.OnUs ? costProfileItem.CostProfile.PointCommission : 0);
        var bankCommissionAmount = (totalBankCommissionRate / 100m) * amount;
        var pfCommissionAmount = pricingProfileItem.PricingProfile.PerTransactionFee +
                                 pricingProfileItem.CommissionRate / 100m * amount;
        var parentMerchantCommissionAmount =
            pricingProfileItem.ParentMerchantCommissionRate / 100m * amount;

        var installmentResponse = new List<InstallmentItem>();
        if (costProfileItem.CostProfile.ProfileSettlementMode == ProfileSettlementMode.PerInstallment)
        {
            installmentResponse = costProfileItem.CostProfileInstallments
                 .Select(b => new InstallmentItem
                 {
                     InstallmentSequence = b.InstallmentSequence,
                     BlockedDayNumber = b.BlockedDayNumber
                 }).ToList();

            installmentResponse.Add(new InstallmentItem
            {
                InstallmentSequence = installment,
                BlockedDayNumber = costProfileItem.BlockedDayNumber
            });
        }

        return new PhysicalPosRouteResponse
        {
            IsSucceed = true,
            CardType = bin?.CardType ?? CardType.International,
            CardTransactionType = cardTransactionType,
            AcquireBank = physicalPos.AcquireBank,
            PricingProfileItem = pricingProfileItem,
            IsAmex = bin is not null && bin.CardBrand == CardBrand.Amex,
            IsInternational = bin == null,
            InstallmentCount = installment,
            BinNumber = binNumber,
            IssuerBankCode = bin?.BankCode ?? 0,
            CurrencyNumber = pricingProfileItem.PricingProfile.Currency.Number,
            Amount = amount,
            PointAmount = pointAmount,
            PointCommissionRate = costProfileItem.CostProfile.PointCommission,//diğer yerlerde onus ise al değilse 0 hesapla yapılmış
            PointCommissionAmount = (pointAmount * costProfileItem.CostProfile.PointCommission) / 100m,//paymentService içerisinde tüm amount üzerinden hesaplanmış
            BankCommissionRate = totalBankCommissionRate,
            BankCommissionAmount = bankCommissionAmount,
            PfCommissionRate = pricingProfileItem.CommissionRate,
            PfCommissionAmount = pfCommissionAmount,
            PfNetCommissionAmount = pfCommissionAmount - bankCommissionAmount,
            PfPerTransactionFee = pricingProfileItem.PricingProfile.PerTransactionFee,
            ParentMerchantCommissionRate = pricingProfileItem.ParentMerchantCommissionRate,
            ParentMerchantCommissionAmount = parentMerchantCommissionAmount,
            ServiceCommissionRate = costProfileItem.CostProfile.ServiceCommission,//diğer yerlerde onus ise al değilse 0 hesapla yapılmış
            ServiceCommissionAmount = (amount * costProfileItem.CostProfile.ServiceCommission) / 100m,
            AmountWithoutCommissions = amount - pfCommissionAmount - parentMerchantCommissionAmount,
            AmountWithoutBankCommission = amount - bankCommissionAmount,
            AmountWithoutParentMerchantCommission = amount - parentMerchantCommissionAmount,
            BsmvAmount = await BsmvAmountCalculateHelper.CalculateBsmvAmount(pfCommissionAmount - bankCommissionAmount, _parameterService),
            BankPaymentDate = transactionDate.Date.AddDays(costProfileItem.BlockedDayNumber + 1),
            PfPaymentDate = await _basePaymentService.CalculatePaymentDateAsync(transactionDate, pricingProfileItem.BlockedDayNumber),
            Installments = installmentResponse,
            CostProfileItemId = costProfileItem.Id,
            ProfileSettlementMode = costProfileItem.CostProfile.ProfileSettlementMode
        };
    }
    private async Task<List<RouterMerchantVposDto>> OnUsPricingAsync(List<RouterMerchantVposDto> merchantVpos, MerchantDto merchant, List<CardLoyalty> loyalty, CardBinDto bin, int installment, decimal amount)
    {
        var merchantVposList = merchantVpos.Where(s =>
               s.RecordStatus == RecordStatus.Active &&
               s.MerchantId == merchant.Id &&
               s.Vpos.VposStatus == VposStatus.Active &&
               (s.Vpos.AcquireBank.BankCode == bin.BankCode ||
               loyalty.Select(s => s.BankCode).ToList().Contains(s.Vpos.AcquireBank.BankCode))).ToList();

        var checkMerchantVpos = await CheckMerchantVposAsync(merchantVposList, installment, amount, BankLimitType.OnUs);

        return checkMerchantVpos;
    }
    private async Task<List<RouterMerchantVposDto>> CheckMerchantVposAsync(List<RouterMerchantVposDto> merchantVposList, int installment, decimal amount, BankLimitType bankLimitType)
    {
        var allMerchantVposList = merchantVposList.ToList();

        var isHealthCheckEnabled = _vaultClient
             .GetSecretValue<bool>("SharedSecrets", "ServiceState", "HealthCheckEnabled");

        if (!isHealthCheckEnabled)
        {
            return merchantVposList;
        }

        try
        {
            if (merchantVposList.Any())
            {
                foreach (var item in merchantVposList.ToList())
                {
                    var bankLimit = new BankLimit();

                    if (installment > 1)
                    {
                        bankLimit = await _bankLimitRepository
                             .GetAll()
                             .Where(b => b.AcquireBankId == item.Vpos.AcquireBankId &&
                                    b.LastValidDate >= DateTime.Now &&
                                    b.RecordStatus == RecordStatus.Active &&
                                    b.BankLimitType == BankLimitType.Installment)
                             .FirstOrDefaultAsync();

                        if (bankLimit is not null)
                        {
                            bankLimit.TotalAmount += amount;

                            if (bankLimit.TotalAmount > bankLimit.MonthlyLimitAmount)
                            {
                                item.IsOptionalPos = true;
                            }
                        }

                        if (bankLimitType != BankLimitType.AllTransaction && item.IsOptionalPos == false)
                        {
                            bankLimit = await _bankLimitRepository
                             .GetAll()
                             .Where(b => b.AcquireBankId == item.Vpos.AcquireBankId &&
                                    b.LastValidDate >= DateTime.Now &&
                                    b.RecordStatus == RecordStatus.Active &&
                                    b.BankLimitType == BankLimitType.AllTransaction)
                             .FirstOrDefaultAsync();

                            if (bankLimit is not null)
                            {
                                bankLimit.TotalAmount += amount;

                                if (bankLimit.TotalAmount > bankLimit.MonthlyLimitAmount)
                                {
                                    item.IsOptionalPos = true;
                                }
                            }

                            if (item.IsOptionalPos == false)
                            {
                                bankLimit = await _bankLimitRepository
                                .GetAll()
                                .Where(b => b.AcquireBankId == item.Vpos.AcquireBankId &&
                                       b.LastValidDate >= DateTime.Now &&
                                       b.RecordStatus == RecordStatus.Active &&
                                       b.BankLimitType == BankLimitType.OnUs)
                                .FirstOrDefaultAsync();

                                if (bankLimit is not null)
                                {
                                    bankLimit.TotalAmount += amount;

                                    if (bankLimit.TotalAmount > bankLimit.MonthlyLimitAmount)
                                    {
                                        item.IsOptionalPos = true;
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        bankLimit = await _bankLimitRepository
                             .GetAll()
                             .Where(b => b.AcquireBankId == item.Vpos.AcquireBankId &&
                                    b.LastValidDate >= DateTime.Now &&
                                    b.RecordStatus == RecordStatus.Active &&
                                    b.BankLimitType == bankLimitType)
                             .FirstOrDefaultAsync();

                        if (bankLimit is not null)
                        {
                            bankLimit.TotalAmount += amount;

                            if (bankLimit.TotalAmount > bankLimit.MonthlyLimitAmount)
                            {
                                item.IsOptionalPos = true;
                            }
                        }

                        if (bankLimitType != BankLimitType.AllTransaction && item.IsOptionalPos == false)
                        {
                            bankLimit = await _bankLimitRepository
                             .GetAll()
                             .Where(b => b.AcquireBankId == item.Vpos.AcquireBankId &&
                                    b.LastValidDate >= DateTime.Now &&
                                    b.RecordStatus == RecordStatus.Active &&
                                    b.BankLimitType == BankLimitType.AllTransaction)
                             .FirstOrDefaultAsync();

                            if (bankLimit is not null)
                            {
                                bankLimit.TotalAmount += amount;

                                if (bankLimit.TotalAmount > bankLimit.MonthlyLimitAmount)
                                {
                                    item.IsOptionalPos = true;
                                }
                            }
                        }
                    }
                }
                foreach (var item in merchantVposList.Where(b => b.IsOptionalPos == false).ToList())
                {
                    var bankHealthCheck = await _bankHealthCheckRepository
                        .GetAll()
                        .Where(b => b.AcquireBankId == item.Vpos.AcquireBankId &&
                               b.HealthCheckType == HealthCheckType.Unhealthy &&
                               b.RecordStatus == RecordStatus.Active &&
                               b.IsHealthCheckAllowed == true)
                        .FirstOrDefaultAsync();

                    if (bankHealthCheck is not null)
                    {
                        item.IsHealthCheckOptionalPos = true;
                    }
                }
            }

            return merchantVposList;

        }
        catch (Exception exception)
        {
            _logger.LogError($"CheckMerchantVposAsyncError : {exception}");
            throw;
        }

    }
    private List<RouterMerchantVposDto> CheckCardBankCodeExistence(List<RouterMerchantVposDto> merchantVposList, MerchantDto merchant, CardBinDto bin)
    {
        var merchantVpos = merchantVposList.Where(merchantVpos =>
            merchantVpos.RecordStatus == RecordStatus.Active &&
            merchantVpos.MerchantId == merchant.Id &&
            merchantVpos.Vpos.VposStatus == VposStatus.Active &&
            merchantVpos.Vpos.AcquireBank != null &&
            merchantVpos.Vpos.AcquireBank.BankCode == bin.BankCode).ToList();

        return merchantVpos;
    }
    private async Task<List<RouterMerchantVposDto>> NotOnUsPricingAsync(List<RouterMerchantVposDto> merchantVpos, MerchantDto merchant, List<CardLoyalty> loyalty, CardBinDto bin, int installment, decimal amount)
    {
        var merchantVposList = merchantVpos.Where(s =>
               s.RecordStatus == RecordStatus.Active &&
               s.MerchantId == merchant.Id &&
               s.Vpos.VposStatus == VposStatus.Active &&
               s.Vpos.AcquireBank.BankCode != bin.BankCode &&
               !loyalty.Select(s => s.BankCode).ToList().Contains(s.Vpos.AcquireBank.BankCode)).ToList();

        var checkMerchantVpos = await CheckMerchantVposAsync(merchantVposList, installment, amount, BankLimitType.NotOnUs);

        return checkMerchantVpos;
    }
    private List<CardLoyalty> GetCardLoyalties(CardBinDto bin, int installment = 0)
    {

        var loyalty = _dbContext.CardLoyalty
                .Where(b => b.Name == bin.CardNetwork.ToString() && b.RecordStatus == RecordStatus.Active)
                .ToList();

        var loyaltyExceptions = _dbContext.CardLoyaltyException
            .Where(s => s.CounterBankCode == bin.BankCode && s.RecordStatus == RecordStatus.Active)
            .ToList();

        if (loyaltyExceptions.Any())
        {
            if (installment > 1)
            {
                var exceptionBankCodes = loyalty
                       .Join(loyaltyExceptions.Where(b => !b.AllowOnUs && !b.AllowInstallment),
                           p => p.BankCode,
                           t => t.BankCode,
                           (p, t) => p.BankCode)
                       .ToList();

                loyalty = loyalty.Where(l => !exceptionBankCodes.Contains(l.BankCode)).ToList();
            }
            else
            {
                var exceptionBankCodes = loyalty
                       .Join(loyaltyExceptions.Where(b => !b.AllowOnUs),
                           p => p.BankCode,
                           t => t.BankCode,
                           (p, t) => p.BankCode)
                       .ToList();

                loyalty = loyalty.Where(l => !exceptionBankCodes.Contains(l.BankCode)).ToList();
            }

            return loyalty;
        }

        return loyalty;
    }
    private async Task<RouteResponse> VposPricing(List<RouterMerchantVposDto> merchantVpos, MerchantDto merchant,
        CardBinDto bin, string currency, decimal amount, CardTransactionType cardTransactionType, int installment = 0)
    {
        var cardType = GetCardType(bin);

        var profileItems = await _costProfileItemRepository.GetAll()
            .Include(s => s.CostProfile)
            .Include(s => s.CostProfile.Vpos)
            .Include(s => s.CostProfile.Vpos.AcquireBank)
            .Include(s => s.CostProfileInstallments)
            .Where(s =>
                s.RecordStatus == RecordStatus.Active &&
                s.CostProfile.ProfileStatus == ProfileStatus.InUse &&
                s.IsActive &&
                s.CostProfile.CurrencyCode == currency &&
                s.CostProfile.PosType == PosType.Virtual &&
                s.ProfileCardType == cardType &&
                s.CardTransactionType == cardTransactionType &&
                merchantVpos.Select(s => s.VposId).ToList().Contains(s.CostProfile.VposId.Value))
            .ToListAsync();

        if (installment <= 12)
        {
            profileItems = profileItems
                .Where(s => s.InstallmentNumber == installment)
                .ToList();
        }
        else
        {
            int endInstallmentNumber = (installment <= 24) ? InstallmentTwentyFour : InstallmentThirtySix;
            profileItems = profileItems
                .Where(s => s.InstallmentNumberEnd == endInstallmentNumber)
                .ToList();
        }

        if (!profileItems.Any())
        {
            return new RouteResponse
            {
                IsSucceed = false
            };
        }

        var itemsWithTotalCommission =
            profileItems.Select(s => new ItemsWithTotalCommission()
            {
                CostProfileItem = s,
                CostProfile = s.CostProfile,
                CardTransactionType = s.CardTransactionType,
                AcquireBank = s.CostProfile.Vpos.AcquireBank,
                Vpos = s.CostProfile.Vpos,
                BlockedDayNumber = s.BlockedDayNumber,
                TotalCommission = (s.CommissionRate + s.CostProfile.ServiceCommission + (cardTransactionType == CardTransactionType.OnUs ? s.CostProfile.PointCommission : 0))
            }).OrderBy(s => s.TotalCommission);

        var totalCommissionValues = itemsWithTotalCommission.Select(item => item.TotalCommission).ToList();

        var minTotalCommission = totalCommissionValues.Min();

        int countOfMinTotalCommission = totalCommissionValues.Count(value => value == minTotalCommission);

        var minComission = new ItemsWithTotalCommission();

        if (countOfMinTotalCommission <= 1)
        {
            minComission = itemsWithTotalCommission.FirstOrDefault();
        }
        else
        {
            var equalTotalTotalCommissions = itemsWithTotalCommission
                    .Where(item => item.TotalCommission == minTotalCommission)
                    .ToList();

            bool allBlockedDayNumberEqual = equalTotalTotalCommissions.All(s => s.BlockedDayNumber == itemsWithTotalCommission.First().BlockedDayNumber);

            if (!allBlockedDayNumberEqual)
            {
                minComission = equalTotalTotalCommissions.MinBy(item => item.BlockedDayNumber);
            }
            else
            {
                var cardBankCodeExistence = CheckCardBankCodeExistence(merchantVpos, merchant, bin);

                if (cardBankCodeExistence.Any())
                {
                    minComission = equalTotalTotalCommissions
                        .FirstOrDefault(s => s.Vpos.Id == cardBankCodeExistence?.FirstOrDefault().Vpos.Id);
                }
                else
                {
                    var equalCommissionVposIds = equalTotalTotalCommissions
                        .Select(item => item.Vpos.Id)
                        .Distinct()
                        .ToList();

                    var filteredMerchantVpos = merchantVpos.Where(mpos => equalCommissionVposIds.Contains(mpos.Vpos.Id))
                        .OrderBy(mv => mv.Priority)
                        .FirstOrDefault();

                    minComission = equalTotalTotalCommissions
                        .FirstOrDefault(s => s.Vpos.Id == filteredMerchantVpos.Vpos.Id);
                }

            }
        }
        var vpos = minComission?.Vpos;

        vpos.VposBankApiInfos = await _vposBankInfoRepository.GetAll()
                    .Include(s => s.Key)
                    .Include(s => s.Vpos.AcquireBank)
                    .Where(s => s.VposId == vpos.Id)
                    .ToListAsync();

        var acqBank = minComission.AcquireBank;
        var transactionType = minComission.CardTransactionType;
        var commissionRate = minComission.TotalCommission;
        var blockedDay = minComission.BlockedDayNumber;
        var isOptionalPos = merchantVpos.Where(b => b.VposId == vpos.Id).FirstOrDefault().IsOptionalPos;
        var isHealthCheckOptionalPos = merchantVpos.Where(b => b.VposId == vpos.Id).FirstOrDefault().IsHealthCheckOptionalPos;

        var installmentResponse = new List<InstallmentItem>();
        if (minComission.CostProfile.ProfileSettlementMode == ProfileSettlementMode.PerInstallment)
        {
            installmentResponse = minComission.CostProfileItem.CostProfileInstallments
                 .Select(b => new InstallmentItem
                 {
                     InstallmentSequence = b.InstallmentSequence,
                     BlockedDayNumber = b.BlockedDayNumber
                 }).ToList();

            installmentResponse.Add(new InstallmentItem
            {
                InstallmentSequence = installment,
                BlockedDayNumber = minComission.BlockedDayNumber
            });
        }

        return new RouteResponse
        {
            AcquireBank = acqBank,
            Vpos = vpos,
            CardTransactionType = transactionType,
            CommissionRate = commissionRate,
            CommissionAmount = (commissionRate / 100m) * amount,
            BlockedDayNumber = blockedDay,
            IsSucceed = true,
            IsOptionalPos = isOptionalPos,
            IsHealthCheckOptionalPos = isHealthCheckOptionalPos,
            Installments = installmentResponse,
            ProfileSettlementMode = minComission.CostProfile.ProfileSettlementMode,
            CostProfileItemId = minComission.CostProfile.Id
        };
    }
    private async Task<RouteResponse> CheckRestrictOwnCardNotOnUs(List<RouterMerchantVposDto> merchantVposList, MerchantDto merchant, string currency, int installment, decimal amount, CardBinDto bin)
    {
        var onUsVpos = merchantVposList.Where(s =>
                        s.RecordStatus == RecordStatus.Active &&
                        s.Vpos.AcquireBank.BankCode == bin.BankCode).ToList();

        if (onUsVpos.Any())
        {
            var vpos = onUsVpos?.FirstOrDefault();

            if (vpos.Vpos.AcquireBank.RestrictOwnCardNotOnUs)
            {
                return await VposPricing(onUsVpos, merchant, bin, currency, amount, CardTransactionType.OnUs, installment);
            }
        }

        return new RouteResponse
        {
            IsSucceed = false
        };
    }
    private class ItemsWithTotalCommission
    {
        public CostProfile CostProfile { get; set; }
        public CostProfileItem CostProfileItem { get; set; }
        public CostProfileInstallment CostProfileInstallments { get; set; }
        public CardTransactionType CardTransactionType { get; set; }
        public AcquireBank AcquireBank { get; set; }
        public Vpos Vpos { get; set; }
        public int BlockedDayNumber { get; set; }
        public decimal TotalCommission { get; set; }
    }
    private static ProfileCardType GetCardType(CardBinDto bin)
    {
        var cardType = ProfileCardType.International;

        if (bin is not null)
        {
            if (bin.CardBrand == CardBrand.Amex) cardType = ProfileCardType.Amex;
            else if (bin.CardType == CardType.Debit) cardType = ProfileCardType.Debit;
            else if (bin.CardType == CardType.Credit) cardType = ProfileCardType.Credit;
        }

        return cardType;
    }
}
