using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Topups.Queries.GetTopupPreview;

public class GetTopupPreviewQuery : IRequest<TopupPreviewResponse>
{
    public string CardNumber { get; set; }
    public decimal Amount { get; set; }
    public string WalletNumber { get; set; }
    public Guid UserId { get; set; }
    public string Currency { get; set; }
}

public class GetTopupPreviewQueryHandler : IRequestHandler<GetTopupPreviewQuery, TopupPreviewResponse>
{
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly ILimitService _limitService;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<PricingProfile> _pricingProfileRepository;
    private readonly IMasterpassService _masterpassService;


    public GetTopupPreviewQueryHandler(IGenericRepository<Wallet> walletRepository,
        IParameterService parameterService,
        ILimitService limitService,
        IGenericRepository<PricingProfile> pricingProfileRepository,
        IMasterpassService masterpassService
)
    {
        _walletRepository = walletRepository;
        _parameterService = parameterService;
        _limitService = limitService;
        _pricingProfileRepository = pricingProfileRepository;
        _masterpassService = masterpassService;
    }

    public async Task<TopupPreviewResponse> Handle(GetTopupPreviewQuery request, CancellationToken cancellationToken)
    {
        var wallet = await _walletRepository
            .GetAll()
            .Include(w => w.Account)
            .ThenInclude(w => w.AccountUsers)
            .Where(w => w.WalletNumber == request.WalletNumber)
            .Where(w => w.Account.AccountUsers.Any(a => a.UserId == request.UserId))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (wallet == null)
        {
            throw new NotFoundException(nameof(Wallet), wallet);
        }

        if (wallet.IsBlocked)
        {
            throw new WalletBlockedException();
        }

        var validateLimit = await ValidateLimitsAsync(wallet, request);

        if (!validateLimit)
        {
            throw new AccountLimitInsufficientException();
        }

        var cardBin = await _masterpassService.GetCardBinByNumberAsync(request.CardNumber);
        var cardType = cardBin.CardType == CardType.Unknown ? CardType.Credit : cardBin.CardType;

        var pricingProfile = await CalculatePricingAsync(new CalculatePricingRequest
        {
            Amount = request.Amount,
            CurrencyCode = request.Currency,
            SenderWalletType = WalletType.Individual,
            TransferType = TransferType.CreditCardTopup,
        },
        cardType);

        if (pricingProfile == null)
        {
            throw new NotFoundException(nameof(PricingProfileItem), pricingProfile);
        }

        return new TopupPreviewResponse
        {
            Amount = pricingProfile.Amount,
            BsmvRate = pricingProfile.BsmvRate,
            BsmvTotal = pricingProfile.BsmvTotal,
            CommissionAmount = pricingProfile.CommissionAmount,
            CommissionRate = pricingProfile.CommissionRate,
            Fee = pricingProfile.Fee,
            CardNumber = request.CardNumber,
            FullName = wallet.Account.Name ?? string.Empty,
            WalletNumber = request.WalletNumber
        };
    }

    private async Task<bool> ValidateLimitsAsync(Wallet wallet, GetTopupPreviewQuery topupPreview)
    {
        var response = await _limitService.IsLimitExceededAsync(new LimitControlRequest
        {
            Amount = topupPreview.Amount,
            CurrencyCode = topupPreview.Currency,
            LimitOperationType = LimitOperationType.Deposit,
            AccountId = wallet.AccountId,
            WalletNumber = wallet.WalletNumber
        });

        return !response.IsLimitExceeded;
    }

    private async Task<CalculatePricingResponse> CalculatePricingAsync(CalculatePricingRequest request, CardType cardType)
    {
        var profile = await _pricingProfileRepository
                .GetAll()
                .Include(s => s.Bank)
                .Include(s => s.Currency)
                .Include(s => s.PricingProfileItems)
            .FirstOrDefaultAsync(s =>
                s.Status == PricingProfileStatus.InUse &&
                s.TransferType == request.TransferType &&
                s.BankCode == request.BankCode &&
                s.CurrencyCode == request.CurrencyCode.ToUpper() &&
                s.CardType == cardType);

        if (profile is null)
        {
            return new CalculatePricingResponse { Amount = request.Amount.ToDecimal2() };
        }

        var item = profile.PricingProfileItems.FirstOrDefault(s =>
            s.MinAmount <= request.Amount &&
            s.MaxAmount >= request.Amount &&
            s.WalletType == request.SenderWalletType);

        if (item is null)
        {
            return new CalculatePricingResponse { Amount = request.Amount.ToDecimal2() };
        }

        var amount = request.Amount.ToDecimal2();
        var fee = item.Fee.ToDecimal2();
        var commissionRate = item.CommissionRate.ToDecimal2();
        var commissionAmount = (amount * (commissionRate / 100m)).ToDecimal2();
        var bsmvRateParameter = await _parameterService.GetParameterAsync("Comission", "BsmvRate");
        var bsmvRate = Convert.ToDecimal(bsmvRateParameter?.ParameterValue);

        var response = new CalculatePricingResponse
        {
            Amount = amount,
            CommissionRate = commissionRate,
            CommissionAmount = commissionAmount,
            Fee = fee,
            BsmvRate = bsmvRate,
            BsmvTotal = ((fee + commissionAmount) * (bsmvRate / 100m)).ToDecimal2(),
        };

        if (response.BsmvTotal == 0 && response.CommissionAmount > 0)
        {
            response.BsmvTotal = 0.01m;
        }

        return response;
    }
}