using System.Security.Cryptography;
using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
public class FinansInit3dModelRequest : FinansPaymentBase
{
    public string PostUrl { get; set; }
    public string OkUrl { get; set; }
    public string FailUrl { get; set; }
    public async Task<string> BuildRequest()
    {
        CardAcceptorStreet = CardAcceptorStreet.Substring(0, Math.Min(CardAcceptorStreet.Length, 32));
        CardAcceptorName = CardAcceptorName.Substring(0, Math.Min(CardAcceptorName.Length, 30));
        string rnd = DateTime.Now.Ticks.ToString();
        string str = MbrId + OrderId + PurchAmount + OkUrl + FailUrl + TxnType + InstallmentCount + rnd + MerchantPass;
        SHA1 sha = SHA1.Create();
        byte[] bytes = Encoding.ASCII.GetBytes(str);
        byte[] hashingbytes = sha.ComputeHash(bytes);
        string hash = Convert.ToBase64String(hashingbytes);

        Rnd = rnd;
        Hash = hash;

        using var client = new HttpClient();
        var formData = new MultipartFormDataContent
        {
            { new StringContent(Pan), "Pan" },
            { new StringContent(Cvv2), "Cvv2" },
            { new StringContent(Expiry), "Expiry" },
            { new StringContent(MbrId), "MbrId" },
            { new StringContent(MerchantId), "MerchantID" },
            { new StringContent(UserCode), "UserCode" },
            { new StringContent(UserPass), "UserPass" },
            { new StringContent(SecureType), "SecureType" },
            { new StringContent(TxnType), "TxnType" },
            { new StringContent(InstallmentCount.ToString()), "InstallmentCount" },
            { new StringContent(CurrencyCode.ToString()), "Currency" },
            { new StringContent(OkUrl), "OkUrl" },
            { new StringContent(FailUrl), "FailUrl" },
            { new StringContent(OrderId), "OrderId" },
            { new StringContent(PurchAmount), "PurchAmount" },
            { new StringContent(BonusAmount), "BonusAmount" },
            { new StringContent(LanguageCode), "Lang" },
            { new StringContent(Rnd), "Rnd" },
            { new StringContent(Hash), "Hash" },
            { new StringContent(PaymentFacilitatorId), "PaymentFacilitatorId" },
            { new StringContent(SubmerchantId), "SubMerchantCode" },
            { new StringContent(SubmerchantMcc), "SubmerchantMCC" },
            { new StringContent(CardAcceptorName), "CardAcceptorName" },
            { new StringContent(CardAcceptorStreet), "CardAcceptorStreet" },
            { new StringContent(CardAcceptorCity), "CardAcceptorCity" },
            { new StringContent(CardAcceptorPostalCode), "CardAcceptorPostalCode" },
            { new StringContent(CardAcceptorCountry), "CardAcceptorCountry" },
            { new StringContent(CardAcceptorState), "CardAcceptorState" },
        };
        var response = await client.PostAsync(PostUrl, formData);
        return await response.Content.ReadAsStringAsync();
    }
}
