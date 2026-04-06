using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;

public class AkbankPaymentNonSecureRequest : AkbankPaymentBase
{
    public decimal NonSecureBonusAmount { get; set; }
    public string BuildRequest()
    {
        if (TxnCode == "1000")
        {
            var akbankNonSecureRequest = new
            {
                version = "1.00",
                txnCode = TxnCode,
                requestDateTime = RequestDateTime,
                randomNumber = RandomNumber,
                terminal = new
                {
                    merchantSafeId = MerchantSafeId,
                    terminalSafeId = TerminalSafeId
                },
                order = new
                {
                    orderId = OrderId
                },
                card = new
                {
                    cardNumber = CardNumber,
                    cvv2 = Cvv2,
                    expireDate = ExpireDate
                },
                transaction = new
                {
                    amount = Amount,
                    currencyCode = CurrencyCode,
                    motoInd = Convert.ToInt32(MotoInd),
                    installCount = InstallCount
                },
                reward = new
                {
                    ccbRewardAmount = decimal.Round(NonSecureBonusAmount, 2, MidpointRounding.AwayFromZero)
                },
                subMerchant = new
                {
                    subMerchantId = SubMerchantId
                }
            };

            return JsonConvert.SerializeObject(akbankNonSecureRequest, Formatting.Indented);
        } 
        else
        {
            var akbankNonSecureRequest = new
            {
                version = "1.00",
                txnCode = TxnCode,
                requestDateTime = RequestDateTime,
                randomNumber = RandomNumber,
                terminal = new
                {
                    merchantSafeId = MerchantSafeId,
                    terminalSafeId = TerminalSafeId
                },
                order = new
                {
                    orderId = OrderId
                },
                card = new
                {
                    cardNumber = CardNumber,
                    cvv2 = Cvv2,
                    expireDate = ExpireDate
                },
                transaction = new
                {
                    amount = Amount,
                    currencyCode = CurrencyCode,
                    motoInd = Convert.ToInt32(MotoInd),
                    installCount = InstallCount
                },
                subMerchant = new
                {
                    subMerchantId = SubMerchantId
                }
            };

            return JsonConvert.SerializeObject(akbankNonSecureRequest, Formatting.Indented);
        }
    }
}
