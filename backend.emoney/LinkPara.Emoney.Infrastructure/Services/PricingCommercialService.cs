using System.Globalization;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.BusinessParameter.Models;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Services;

public class PricingCommercialService : IPricingCommercialService
{
    private readonly IParameterService _parameterService;
    private readonly IPushNotificationSender _pushNotificationSender;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IUserService _userService;
    private readonly ILogger<PricingCommercialService> _logger;

    public PricingCommercialService
    (
        IParameterService parameterService,
        IPushNotificationSender pushNotificationSender,
        IServiceScopeFactory scopeFactory,
        IUserService userService,
        ILogger<PricingCommercialService> logger)
    {
        _parameterService = parameterService;
        _pushNotificationSender = pushNotificationSender;
        _scopeFactory = scopeFactory;
        _userService = userService;
        _logger = logger;
    }

    public async Task<bool> CheckIfAccountIsCommercialNowAsync(string currencyCode, PricingCommercialType pricingCommercialType, Guid receiverAccountId, bool ownAccount)
    {
        if (ownAccount)
        {
            return false;
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var commercialPricingValues =
            await dbContext.PricingCommercial
                .FirstOrDefaultAsync(s =>
                    s.PricingCommercialStatus == PricingCommercialStatus.InUse
                    && s.CurrencyCode == currencyCode
                    && (s.PricingCommercialType == PricingCommercialType.All
                        || s.PricingCommercialType == pricingCommercialType));

        if (commercialPricingValues is null)
        {
            return false;
        }

        var query = dbContext.AccountActivity
                .Where(a =>
                    a.AccountId == receiverAccountId
                    && a.RecordStatus == RecordStatus.Active
                    && a.TransactionDirection == TransactionDirection.MoneyIn
                    && a.Month == DateTime.Now.Month
                    && a.Year == DateTime.Now.Year
                    && a.OwnAccount == false);

        if (commercialPricingValues.PricingCommercialType != PricingCommercialType.All)
        {
            query = query.Where(a => a.TransferType == pricingCommercialType.ToString());
        }

        var accountActivities = await query.ToListAsync();

        if (!accountActivities.Any())
        {
            return false;
        }

        var userLimitWithAmount = commercialPricingValues.MaxDistinctSenderCountWithAmount;
        var monthlyAmountLimit = commercialPricingValues.MaxDistinctSenderAmount;
        var userLimitWithoutAmount = commercialPricingValues.MaxDistinctSenderCount;

        var currentDistinctSenderCount = accountActivities
            .Select(a => a.Sender)
            .Distinct()
            .Count();

        var currentAmount = accountActivities
            .Select(a => a.Amount)
            .Sum();

        return HasExceededMaxDistinctSenderCount(currentDistinctSenderCount, userLimitWithoutAmount)
               || HasExceededMaxDistinctSenderAndAmount(currentAmount, monthlyAmountLimit, currentDistinctSenderCount, userLimitWithAmount);
    }

    public async Task<CalculatePricingResponse> CalculateCustomPricingAsync(decimal amount, decimal fee,
        decimal commissionRate)
    {
        var commissionAmount = (amount * (commissionRate / 100m)).ToDecimal2();
        var bsmvRateParameter = await _parameterService.GetParameterAsync("Comission", "BsmvRate");
        var bsmvRate = Convert.ToDecimal(bsmvRateParameter?.ParameterValue);

        var response = new CalculatePricingResponse
        {
            Amount = amount.ToDecimal2(),
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

    private static bool HasExceededMaxDistinctSenderAndAmount(decimal currentAmount, decimal monthlyAmountLimit, int currentDistinctSenderCount, int userLimitWithAmount)
    {
        return currentAmount >= monthlyAmountLimit &&
               currentDistinctSenderCount >= userLimitWithAmount;
    }

    private static bool HasExceededMaxDistinctSenderCount(int currentDistinctSenderCount, int userLimitWithoutAmount)
    {
        return currentDistinctSenderCount >= userLimitWithoutAmount;
    }

    public async Task SendCommercialInfoPushNotificationAsync(Account receiverAccount, PricingCommercial pricingCommercial)
    {
        var maxDistinctSenderCount = pricingCommercial.MaxDistinctSenderCount;
        var maxDistinctSenderCountWithAmount = pricingCommercial.MaxDistinctSenderCountWithAmount;
        var maxDistinctSenderAmount = pricingCommercial.MaxDistinctSenderAmount;

        var receiverUserIdList = receiverAccount.AccountUsers
            .Select(x => new NotificationUserInfo
            {
                UserId = x.UserId,
                FirstName = x.Firstname,
                LastName = x.Lastname,
            })
            .ToList();

        var receiverUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(
            new GetUserDeviceInfoRequest()
            {
                UserIdList = receiverUserIdList
                    .Select(x => x.UserId)
                    .ToList()
            });

        var receiverNotificationRequest = new SendPushNotification
        {
            TemplateName = "CommercialPricingNotification",
            TemplateParameters = new Dictionary<string, string>
            {
                {"maxDistinctSenderCount", maxDistinctSenderCount.ToString()},
                {"maxDistinctSenderCountWithAmount", maxDistinctSenderCountWithAmount.ToString()},
                {"maxDistinctSenderAmount", maxDistinctSenderAmount.ToString("N2")},
                {"commission", pricingCommercial.CommissionRate.ToString("N2")}
            },
            Tokens = receiverUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = receiverUserIdList
        };

        await _pushNotificationSender.SendPushNotificationAsync(receiverNotificationRequest);
    }

    public async Task<PricingCommercial> GetPricingCommercialRateAsync(string currencyCode, PricingCommercialType pricingCommercialType)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
        return await dbContext.PricingCommercial
            .FirstOrDefaultAsync(s =>
                s.PricingCommercialStatus == PricingCommercialStatus.InUse
                && s.CurrencyCode == currencyCode
                && (s.PricingCommercialType == pricingCommercialType
                    || s.PricingCommercialType == PricingCommercialType.All));
    }

    public async Task<bool> IsGreaterThanMinAmountLimit(decimal amount)
    {
        ParameterDto minAmountLimitParameter = null;

        try
        {
            minAmountLimitParameter =
                await _parameterService.GetParameterAsync("EmoneyCommercialPricing", "MinAmountLimit");
        }
        catch (Exception exception)
        {
            _logger.LogError($"IsGreaterThanMinAmountLimit Exception: {exception}");
        }

        if (minAmountLimitParameter == null || string.IsNullOrEmpty(minAmountLimitParameter.ParameterValue))
        {
            return true;
        }

        var isMinAmountLimitParsed = decimal.TryParse(minAmountLimitParameter.ParameterValue, out var minAmountLimit);

        if (isMinAmountLimitParsed)
        {
            return amount > minAmountLimit;
        }

        return true;
    }
}