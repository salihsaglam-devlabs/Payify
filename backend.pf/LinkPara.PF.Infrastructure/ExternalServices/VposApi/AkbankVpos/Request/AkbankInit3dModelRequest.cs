namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;

public  class AkbankInit3dModelRequest : AkbankPaymentBase
{  
    public async Task<string> BuildRequest()
    {
        string rnd = DateTime.Now.Ticks.ToString();
        string hashItems = "";

        if (TxnCode == "3000")
        {
            hashItems = PaymentModel + TxnCode + MerchantSafeId + TerminalSafeId + OrderId + Lang + Amount + BonusAmount + CurrencyCode + InstallCount
            + OkUrl + FailUrl + SubMerchantId + CardNumber + ExpireDate + Cvv2 + RandomNumber + RequestDateTime;
        } else
        {
            hashItems = PaymentModel + TxnCode + MerchantSafeId + TerminalSafeId + OrderId + Lang + Amount + CurrencyCode + InstallCount
            + OkUrl + FailUrl + SubMerchantId + CardNumber + ExpireDate + Cvv2 + RandomNumber + RequestDateTime;
        }
        
        string hashedMessage = VposHelper.HashToString(hashItems, SecretKey);

        var formData = new Dictionary<string, string>
        {
            { "paymentModel", PaymentModel },
            { "txnCode", TxnCode },
            { "merchantSafeId", MerchantSafeId },
            { "terminalSafeId", TerminalSafeId },
            { "orderId", OrderId },
            { "lang", Lang },
            { "amount", Amount },
            { "currencyCode", CurrencyCode.ToString() },
            { "installCount", InstallCount.ToString() },
            { "okUrl", OkUrl },
            { "failUrl", FailUrl },
            { "subMerchantId", SubMerchantId },
            { "creditCard", CardNumber },
            { "expiredDate", ExpireDate },
            { "cvv", Cvv2 },
            { "randomNumber", RandomNumber },
            { "requestDateTime", RequestDateTime },
            { "hash", hashedMessage }
        };

        if (TxnCode == "3000")
            formData.Add("ccbRewardAmount", BonusAmount);

        using var client = new HttpClient();
        var requestContent = new FormUrlEncodedContent(formData);
        var response = await client.PostAsync(PostUrl, requestContent);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
