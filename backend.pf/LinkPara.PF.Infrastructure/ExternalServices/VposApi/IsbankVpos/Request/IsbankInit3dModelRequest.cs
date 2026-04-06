using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Request;

public class IsbankInit3dModelRequest : IsbankPaymentRequestBase
{
    public string OkUrl { get; set; }
    public string FailUrl { get; set; }
    public string InitUrl { get; set; }
    public string ClientIp { get; set; }
    public string MerchantPassword { get; set; }
    public string BuildRequest()
    {
        SubMerchantName = SubMerchantName.Substring(0, Math.Min(SubMerchantName.Length, 30));
        Rnd = GenerateRandom10DigitNumber();
        var hashAmount = Convert.ToInt32(Amount * 100m);
        var hashVal = MerchantNumber + "-" +
                         OrderNumber + "-" +
                         hashAmount+ "-" +
                         OkUrl + "-" +
                         FailUrl + "-" +
                         Rnd + "-" +
                         MerchantPassword;

        Hash = VposHelper.GetSHA512String(hashVal);

        var stAmount = Amount.ToString("0.00", CultureInfo.InvariantCulture);
        var stPointAmount = PointAmount.ToString("0.00", CultureInfo.InvariantCulture);
        var expireMonth = CardExpireMonth.ToString("D2");
        
        var request = new StringBuilder();

        request.Append("<html>");
        request.Append("<head></head>");
        request.Append("<body onload=\"document.ThreeDPostForm.submit()\">");
        request.Append($"<form name=\"ThreeDPostForm\" method=\"POST\" action=\"{InitUrl}\">");
        request.Append($"<input type=\"hidden\" name=\"MerchantNumber\" value=\"{MerchantNumber.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"Rnd\" value=\"{Rnd.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"SuccessUrl\" value=\"{OkUrl.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"FailUrl\" value=\"{FailUrl.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"FlowType\" value=\"AuthenticationOnly\"/>");
        request.Append($"<input type=\"hidden\" name=\"OrderNumber\" value=\"{OrderNumber.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"CardholderName\" value=\"{CardHolderName.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"CardNumber\" value=\"{CardNumber.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"CardExpiryMonth\" value=\"{expireMonth.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"CardExpiryYear\" value=\"{CardExpireYear}\"/>");
        request.Append($"<input type=\"hidden\" name=\"Cvv\" value=\"{Cvv.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"Amount\" value=\"{stAmount.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"CurrencyCode\" value=\"{Currency}\"/>");
        request.Append($"<input type=\"hidden\" name=\"InstallCount\" value=\"{Installment}\"/>");
        request.Append($"<input type=\"hidden\" name=\"ClientIp\" value=\"{ClientIp.Trim()}\"/>");
        request.Append($"<input type=\"hidden\" name=\"TransactionType\" value=\"0{TransactionType}\"/>");
        request.Append($"<input type=\"hidden\" name=\"PaymentFacilitatorSubMerchantName\" value=\"{SubMerchantName}\">");
        request.Append($"<input type=\"hidden\" name=\"PaymentFacilitatorSubMerchantId\" value=\"{SubMerchantId}\">");
        request.Append($"<input type=\"hidden\" name=\"PaymentFacilitatorSubMerchantMcc\" value=\"{SubMerchantMcc}\">");
        request.Append($"<input type=\"hidden\" name=\"PaymentFacilitatorSubMerchantCitizenId\" value=\"{SubMerchantCitizenId}\">");
        request.Append($"<input type=\"hidden\" name=\"PaymentFacilitatorSubMerchantCity\" value=\"{SubMerchantCity}\">");
        request.Append($"<input type=\"hidden\" name=\"PaymentFacilitatorSubMerchantPostalCode\" value=\"{SubMerchantPostalCode}\">");
        request.Append($"<input type=\"hidden\" name=\"PaymentFacilitatorSubMerchantUrl\" value=\"{SubMerchantUrl}\">");

        if (PointAmount > 0)
        {
            request.Append($"<input type=\"hidden\" name=\"PointAmountToUse\" value=\"{stPointAmount.Trim()}\"/>");
        }

        request.Append($"<input type=\"hidden\" name=\"Hash\" value=\"{Hash.Trim()}\"/>");
        request.Append("</form>");
        request.Append("</body>");
        request.Append("</html>");

        return request.Replace("\r\n", " ").ToString();
    }
    
    private static string GenerateRandom10DigitNumber()
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] bytes = new byte[8];
        rng.GetBytes(bytes);
        long number = Math.Abs(BitConverter.ToInt64(bytes, 0)) % 10000000000L;
        return number.ToString("D10");
    }
}