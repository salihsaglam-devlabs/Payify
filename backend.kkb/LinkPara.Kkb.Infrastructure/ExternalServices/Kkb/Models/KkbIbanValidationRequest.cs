using LinkPara.Security.Helpers;
using System.Security;
using System.Text;


namespace LinkPara.Kkb.Infrastructure.ExternalServices.Kkb.Models;

public static class KkbIbanValidationRequest
{
    public static string CreateRequestXmlForIbanValidation(string userName, SecureString password, string iban, string identityNo, bool isNewMethod)
    {
        var result = new StringBuilder();
        result.Append("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:wsv2=\"http://wsv2.ers.kkb.com.tr/\">");
        result.Append("<soapenv:Header><wsse:Security xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\" xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\">");
        result.Append($"<wsse:UsernameToken wsu:Id=\"{userName}\">");
        result.Append($"<wsse:Username>{userName}</wsse:Username>");
        result.Append($"<wsse:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">{password.ToPlainString()}</wsse:Password>");
        result.Append("</wsse:UsernameToken>");
        result.Append("</wsse:Security>");
        result.Append("</soapenv:Header>");
        result.Append("<soapenv:Body>");
        if (isNewMethod)
        {
            result.Append("<wsv2:yeniBsgIbanDogrulama>");
        }
        else
        {
            result.Append("<wsv2:bsgIbanDogrulama>");
        }
        result.Append("<IbanDogrulamaInputBean>");
        result.Append("<clientIp></clientIp>");
        result.Append("<kanal>7</kanal>");
        result.Append("<kullanici></kullanici>");
        result.Append($"<ibanNo>{iban}</ibanNo>");
        result.Append($"<kimlikNo>{identityNo}</kimlikNo>");
        result.Append("<kullaniciAdSoyad></kullaniciAdSoyad>");
        result.Append("<kullaniciKodu></kullaniciKodu>");
        result.Append("<subeBayiAdi></subeBayiAdi>");
        result.Append("<subeBayiKodu></subeBayiKodu>");
        result.Append("</IbanDogrulamaInputBean>");
        if (isNewMethod)
        {
            result.Append("</wsv2:yeniBsgIbanDogrulama>");
        }
        else
        {
            result.Append("</wsv2:bsgIbanDogrulama>");
        }
        result.Append("</soapenv:Body>");
        result.Append("</soapenv:Envelope>");

        return result.ToString();
    }
}
