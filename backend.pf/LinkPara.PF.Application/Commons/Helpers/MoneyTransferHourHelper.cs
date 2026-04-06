using LinkPara.HttpProviders.BusinessParameter;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Commons.Helpers;

public static class MoneyTransferHourHelper
{
    public static async Task<TimeSpan> GetMoneyTransferHourAsync(IParameterService parameterService, ILogger logger)
    {
        const int defaultTransferHour = 10;
        const int defaultTransferMinute = 00;

        try
        {
            var hour = int.Parse(
                (await parameterService.GetParametersAsync("PostingParams"))
                .FirstOrDefault(w => w.ParameterCode == "MoneyTransferStartHour")
                ?.ParameterValue
            );
            
            var minute = int.Parse(
                (await parameterService.GetParametersAsync("PostingParams"))
                .FirstOrDefault(w => w.ParameterCode == "MoneyTransferStartMinute")
                ?.ParameterValue
            );

            return new TimeSpan(hour, minute, 0);
        }
        catch
        {
            logger.LogError($"PostingMoneyTransferStartHourNotDefined. ContinueWithDefault: {defaultTransferHour}");

            return new TimeSpan(defaultTransferHour, defaultTransferMinute, 0);
        }
    }
    
    public static async Task<int> GetMoneyHolidayTransferAmountThresholdAsync(IParameterService parameterService, ILogger logger)
    {
        const int defaultHolidayTransferAmountThreshold = 100_000;

        try
        {
            var amountThreshold = int.Parse(
                (await parameterService.GetParametersAsync("PostingParams"))
                .FirstOrDefault(w => w.ParameterCode == "HolidayTransferAmountThreshold")
                ?.ParameterValue
            );
           
            return amountThreshold;
        }
        catch
        {
            logger.LogError($"PostingMoneyTransferStartHourNotDefined. ContinueWithDefault: {defaultHolidayTransferAmountThreshold}");
            return defaultHolidayTransferAmountThreshold;
        }
    }
}