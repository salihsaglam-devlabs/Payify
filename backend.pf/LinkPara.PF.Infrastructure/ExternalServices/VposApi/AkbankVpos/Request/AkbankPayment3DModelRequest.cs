using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;

public class AkbankPayment3DModelRequest : AkbankPaymentBase
{
    public string SecureId { get; set; }
    public string SecureEcomInd { get; set; }
    public string SecureData { get; set; }
    public string SecureMd { get; set; }
    public decimal PointAmount { get; set; }
    public string BuildRequest()
    {
        if (TxnCode == "1000")
        {
            var akbank3dModelRequest = new
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
                transaction = new
                {
                    amount = Amount,
                    currencyCode = CurrencyCode,
                    motoInd = Convert.ToInt32(MotoInd),
                    installCount = InstallCount
                },
                secureTransaction = new
                {
                    secureId = SecureId,
                    secureEcomInd = SecureEcomInd,
                    secureData = SecureData,
                    secureMd = SecureMd,
                },
                reward = new
                {
                    ccbRewardAmount = PointAmount
                },
                subMerchant = new
                {
                    subMerchantId = SubMerchantId
                }
            };

            return JsonConvert.SerializeObject(akbank3dModelRequest, Formatting.Indented);
        } 
        else
        {
            var akbank3dModelRequest = new
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
                transaction = new
                {
                    amount = Amount,
                    currencyCode = CurrencyCode,
                    motoInd = Convert.ToInt32(MotoInd),
                    installCount = InstallCount
                },
                secureTransaction = new
                {
                    secureId = SecureId,
                    secureEcomInd = SecureEcomInd,
                    secureData = SecureData,
                    secureMd = SecureMd,
                },
                subMerchant = new
                {
                    subMerchantId = SubMerchantId
                }
            };

            return JsonConvert.SerializeObject(akbank3dModelRequest, Formatting.Indented);
        }   
    }
}
