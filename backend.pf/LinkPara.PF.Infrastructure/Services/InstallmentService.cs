using AutoMapper;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.Installments;
using LinkPara.PF.Application.Features.Installments.Queries.CalculateInstallmentPricing;
using LinkPara.PF.Application.Features.Installments.Queries.GetManualPaymentPageInstallments;
using LinkPara.PF.Application.Features.Links;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Services;

public class InstallmentService : IInstallmentService
{
    private readonly PfDbContext _dbContext;
    private readonly ICardBinService _binService;

    public InstallmentService(PfDbContext dbContext, ICardBinService binService, IMapper mapper
    )
    {
        _dbContext = dbContext;
        _binService = binService;
    }

    public async Task<InstallmentPricingResponse> GetInstallmentPricingAsync(CalculateInstallmentPricingQuery request)
    {
        var merchant = await GetMerchantInfoAsync(request.MerchantNumber);

        var pricingProfile = await GetPricingProfileAsync(merchant.PricingProfileNumber);

        InstallmentPricingResponse response;

        if (!string.IsNullOrEmpty(request.BinNumber))
        {
            response = await GetInstallmentPricingByCardBinAsync(request, pricingProfile);
        }
        else
        {
            response = await GetInstallmentPricingByMerchantAsync(request, pricingProfile, merchant);
        }

        response.ConversationId = request.ConversationId;

        return response;
    }

    private async Task<InstallmentPricingResponse> GetInstallmentPricingByCardBinAsync(CalculateInstallmentPricingQuery request,
        PricingProfile pricingProfile)
    {
        var cardBinDto = await _binService.GetByNumberAsync(request.BinNumber);

        var hasLoyaltyForInstallment = false;
        bool hasLoyaltyForCash;
        var cardNetwork = CardNetwork.Unknown;
        var cardType = CardType.Unknown;
        var isMerchantPos = true;
        var isMerchantPosLoyalty = true;

        if (cardBinDto is null)
        {
            hasLoyaltyForCash = true;

            var pricingProfileItems = pricingProfile.PricingProfileItems
                .Where(s => s.ProfileCardType == ProfileCardType.International).ToList();

            pricingProfile.PricingProfileItems = pricingProfileItems;
        }
        else
        {
            cardNetwork = cardBinDto.CardNetwork;
            cardType = cardBinDto.CardType;

            hasLoyaltyForInstallment = HasCardLoyalty(cardBinDto.CardNetwork.ToString(), true, cardBinDto.BankCode);
            hasLoyaltyForCash = HasCardLoyalty(cardBinDto.CardNetwork.ToString(), false, cardBinDto.BankCode);

            isMerchantPos = await IsMerchantVpos(request.MerchantNumber, cardBinDto.BankCode);
            isMerchantPosLoyalty = await IsMerchantVposLoyalty(request.MerchantNumber, cardNetwork);

        }

        var loyaltyInstallmentPricing = GetLoyaltyInstallmentPricing(request, pricingProfile, hasLoyaltyForCash, hasLoyaltyForInstallment, cardNetwork, cardType, isMerchantPos, isMerchantPosLoyalty);

        var installmentPricingByLoyaltyResponse = new InstallmentPricingResponse()
        {
            MerchantNumber = request.MerchantNumber,
            LoyaltyInstallmentPricing = new List<LoyaltyInstallmentPricing>
            {
                loyaltyInstallmentPricing
            },
            IsSucceed = true
        };

        return installmentPricingByLoyaltyResponse;
    }

    private LoyaltyInstallmentPricing GetLoyaltyInstallmentPricing(CalculateInstallmentPricingQuery request, PricingProfile pricingProfile,
        bool hasLoyaltyForCash, bool hasLoyaltyForInstallment, CardNetwork cardNetwork, CardType cardType, bool isMerchantPos, bool isMerchantPosLoyalty)
    {
        var installmentPricings = new List<InstallmentPricing>();
        var pricingProfileItems = pricingProfile.PricingProfileItems.ToList();

        if (cardNetwork == CardNetwork.Unknown && cardType != CardType.Unknown)
        {
            if (cardType == CardType.Debit)
            {
                pricingProfileItems = pricingProfileItems.Where(b => b.InstallmentNumberEnd <= 1 && b.ProfileCardType == ProfileCardType.Debit).ToList();
            }
            else
            {
                pricingProfileItems = pricingProfileItems.Where(b => b.InstallmentNumberEnd <= 1 && b.ProfileCardType == ProfileCardType.Credit).ToList();
            }

            var pricing = CalculateInstallmentPricingAsync(pricingProfileItems.FirstOrDefault(), request.Amount);
            installmentPricings.Add(pricing);
        }
        else
        {
            if (cardType == CardType.Debit)
            {
                pricingProfileItems = pricingProfileItems.Where(b => b.ProfileCardType == ProfileCardType.Debit).ToList();
            }
            else if (cardType == CardType.Credit)
            {
                pricingProfileItems = pricingProfileItems.Where(b => b.ProfileCardType == ProfileCardType.Credit).ToList();

                if (isMerchantPos == false && isMerchantPosLoyalty == false)
                {
                    pricingProfileItems = pricingProfileItems.Where(b => b.InstallmentNumberEnd <= 1).ToList();
                }
            }

            foreach (var pricingProfileItem in pricingProfileItems)
            {
                if ((pricingProfileItem.InstallmentNumberEnd <= 1 && hasLoyaltyForCash)
                    || (pricingProfileItem.InstallmentNumberEnd > 1 && hasLoyaltyForInstallment))
                {
                    var pricing = CalculateInstallmentPricingAsync(pricingProfileItem, request.Amount);
                    installmentPricings.Add(pricing);
                }
            }
        }

        if (installmentPricings.Any())
        {
            installmentPricings = installmentPricings
                .OrderBy(s => s.ProfileCardType)
                .ThenBy(s => s.InstallmentNumberEnd)
                .ToList();
        }

        return new LoyaltyInstallmentPricing
        {
            CardNetwork = cardNetwork,
            InstallmentPricings = installmentPricings
        };
    }

    private async Task<InstallmentPricingResponse> GetInstallmentPricingByMerchantAsync(CalculateInstallmentPricingQuery request,
        PricingProfile pricingProfile, Merchant merchant)
    {
        var merchantCardNetworks = await GetMerchantCardNetworksAsync(merchant.Id);

        if (merchantCardNetworks.Any())
        {
            var loyaltyInstallmentPricingList = new List<LoyaltyInstallmentPricing>();

            foreach (var merchantCardNetwork in merchantCardNetworks)
            {
                var hasLoyaltyForInstallment = HasCardLoyalty(merchantCardNetwork.ToString(), true, 0);
                var hasLoyaltyForCash = HasCardLoyalty(merchantCardNetwork.ToString(), false, 0);

                var loyaltyInstallmentPricing = GetLoyaltyInstallmentPricing(request, pricingProfile, hasLoyaltyForCash, hasLoyaltyForInstallment,
                    merchantCardNetwork, CardType.Unknown, false, false);

                if (loyaltyInstallmentPricing.InstallmentPricings != null && loyaltyInstallmentPricing.InstallmentPricings.Any())
                {
                    loyaltyInstallmentPricingList.Add(loyaltyInstallmentPricing);
                }
            }

            return new InstallmentPricingResponse
            {
                MerchantNumber = request.MerchantNumber,
                LoyaltyInstallmentPricing = loyaltyInstallmentPricingList,
                IsSucceed = true
            };
        }

        return new InstallmentPricingResponse
        {
            MerchantNumber = request.MerchantNumber,
            IsSucceed = true
        };
    }

    private InstallmentPricing CalculateInstallmentPricingAsync(PricingProfileItem pricingProfileItem, decimal amount)
    {
        var amountWithPricing = Math.Round(amount / (1 - (pricingProfileItem.CommissionRate / 100)), 2);
        var commissionAmount = Math.Round((amountWithPricing - amount), 2);

        return new InstallmentPricing
        {
            ProfileCardType = pricingProfileItem.ProfileCardType,
            InstallmentNumber = pricingProfileItem.InstallmentNumber,
            InstallmentNumberEnd = pricingProfileItem.InstallmentNumberEnd,
            CommissionRate = pricingProfileItem.CommissionRate,
            Amount = amount,
            CommissionAmount = commissionAmount,
            TotalAmount = amountWithPricing,
            BlockedDayNumber = pricingProfileItem.BlockedDayNumber,
            IsActive = pricingProfileItem.IsActive
        };
    }

    private async Task<PricingProfile> GetPricingProfileAsync(string pricingProfileNumber)
    {
        var pricingProfile = await _dbContext.PricingProfile
            .Where(p =>
                p.PricingProfileNumber == pricingProfileNumber
                && p.ProfileStatus == ProfileStatus.InUse
                && p.RecordStatus == RecordStatus.Active)
            .Include(s => s.PricingProfileItems)
            .FirstOrDefaultAsync();

        if (pricingProfile is null)
        {
            throw new NotFoundException(typeof(PricingProfile).ToString());
        }

        return pricingProfile;
    }

    private async Task<Merchant> GetMerchantInfoAsync(string merchantNumber)
    {
        var merchant = await _dbContext.Merchant
            .Where(m =>
                m.Number == merchantNumber
                && m.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync();

        if (merchant == null)
        {
            throw new NotFoundException(typeof(Merchant).ToString());
        }

        return merchant;
    }

    private async Task<bool> IsMerchantVpos(string merchantNumber, int binBankCode)
    {
        var merchant = await _dbContext.Merchant
            .Include(b=>b.MerchantVposList.Where(x=>x.RecordStatus == RecordStatus.Active))
            .ThenInclude(b=>b.Vpos).ThenInclude(c=>c.AcquireBank)
           .Where(m =>
               m.Number == merchantNumber
               && m.RecordStatus == RecordStatus.Active)
           .FirstOrDefaultAsync();

        if (merchant == null)
        {
            throw new NotFoundException(typeof(Merchant).ToString());
        }

        return merchant.MerchantVposList
                  .Any(mv => mv.Vpos?.AcquireBank?.BankCode == binBankCode);
    }

    private async Task<bool> IsMerchantVposLoyalty(string merchantNumber, CardNetwork binCardNetwork)
    {
        var merchant = await _dbContext.Merchant
            .Include(b => b.MerchantVposList.Where(x => x.RecordStatus == RecordStatus.Active))
            .ThenInclude(b => b.Vpos).ThenInclude(c => c.AcquireBank)
           .Where(m =>
               m.Number == merchantNumber
               && m.RecordStatus == RecordStatus.Active)
           .FirstOrDefaultAsync();

        if (merchant == null)
        {
            throw new NotFoundException(typeof(Merchant).ToString());
        }

        var merchantBankCodes = merchant.MerchantVposList
            .Select(mv => mv.Vpos?.AcquireBank?.BankCode)
            .Where(code => code != null)
            .Distinct()
            .ToList();

        var loyalties = await _dbContext.CardLoyalty
            .Where(cl => cl.Name == binCardNetwork.ToString() && cl.RecordStatus == RecordStatus.Active)
            .ToListAsync();

        bool hasLoyalty = loyalties.Any(l => merchantBankCodes.Contains(l.BankCode));

        return hasLoyalty;
    }

    private async Task<List<CardNetwork>> GetMerchantCardNetworksAsync(Guid merchantId)
    {
        var merchantVpos = _dbContext.MerchantVpos
            .Include(m => m.Vpos)
            .Include(m => m.Vpos.AcquireBank)
            .Where(v => v.MerchantId == merchantId && v.RecordStatus == RecordStatus.Active);

        List<CardNetwork> cardNetworks = null;

        if (merchantVpos.Any())
        {
            cardNetworks = await merchantVpos
                .Select(m => m.Vpos.AcquireBank.CardNetwork)
                .Distinct()
                .ToListAsync();
        }

        return cardNetworks;
    }

    private bool HasCardLoyalty(string cardNetwork, bool hasInstallment, int binBankCode)
    {

        var loyalty = _dbContext.CardLoyalty
            .Where(b => b.Name == cardNetwork && b.RecordStatus == RecordStatus.Active)
            .ToList();
        
        var loyaltyExceptionsQuery = _dbContext.CardLoyaltyException
            .Where(s => s.RecordStatus == RecordStatus.Active);
        
        var loyaltyExceptions = binBankCode == 0 ? loyaltyExceptionsQuery.ToList() : loyaltyExceptionsQuery.Where(s => s.CounterBankCode == binBankCode).ToList();

        if (loyaltyExceptions.Any())
        {
            if (hasInstallment)
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

            return loyalty.Any();
        }

        return loyalty.Any();
    }

    public async Task<ManualPaymentPageInstallmentsResponse> GetManualPaymentPageInstallmentsAsync(GetManualPaymentPageInstallmentsQuery request)
    {
        var merchant = await _dbContext.Merchant
            .Where(m =>
                m.Id == request.MerchantId
                && m.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync();

        if (merchant is null)
        {
            throw new NotFoundException(nameof(merchant));
        }

        var mppInstallmentsResponse = new ManualPaymentPageInstallmentsResponse
        {
            Is3dRequired = merchant.IsManuelPayment3dRequired
        };

        if (!merchant.InstallmentAllowed)
        {
            return mppInstallmentsResponse;
        }

        var pricingProfile = await GetPricingProfileAsync(merchant.PricingProfileNumber);

        mppInstallmentsResponse.AvailableInstallmentCounts = pricingProfile.PricingProfileItems
                    .Where(item => item.InstallmentNumberEnd != 0 && item.IsActive)
                    .OrderBy(item => item.InstallmentNumberEnd)
                    .Select(item => item.InstallmentNumberEnd)
                    .ToList();

        return mppInstallmentsResponse;
    }
}