using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Banking.Enums;

namespace LinkPara.PF.Infrastructure.Services;

public class OrderService : IOrderService
{
    public async Task<string> GenerateOrderNumberAsync(int bankCode)
    {
        var orderNumber = string.Empty;

        switch (bankCode)
        {
            case (int)BankCode.Denizbank:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }

            case (int)BankCode.VakifBank:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
            case (int)BankCode.IsBank:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
            case (int)BankCode.Ziraat:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
            case (int)BankCode.Akbank:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
            case (int)BankCode.Halkbank:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
            case (int)BankCode.KuveytTurk:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
            case (int)BankCode.Finansbank:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
            case (int)BankCode.SekerBank:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
            case (int)BankCode.YapiKredi:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
            case (int)BankCode.TurkiyeFinansKatilim:
                {
                    orderNumber = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString().PadLeft(20, '0');
                    break;
                }
        }

        return await Task.FromResult(orderNumber);
    }
}
