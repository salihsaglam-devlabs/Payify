using LinkPara.HttpProviders.BusinessParameter;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Commons.Helpers
{
    public static class BsmvAmountCalculateHelper
    {
        public static async Task<decimal> CalculateBsmvAmount(decimal profit, IParameterService _parameterService)
        {
            decimal bsmvRate = 0m;
            decimal bsmvAmount = 0m;

            try
            {
                var bsmvRateParameter = await _parameterService.GetParameterAsync("Comission", "BsmvRate");
                if (bsmvRateParameter is null || !decimal.TryParse(bsmvRateParameter.ParameterValue, out bsmvRate))
                {
                    bsmvRate = 5.0m;
                }
            }
            catch (Exception)
            {
                bsmvRate = 5.0m;
            }

            if (profit > 0)
            {
                var bsmvCommision = (bsmvRate / 100.0m) + 1.0m;

                bsmvAmount = profit / bsmvCommision * (bsmvRate / 100.0m);
            }

            return bsmvAmount;
        }
    }
}
