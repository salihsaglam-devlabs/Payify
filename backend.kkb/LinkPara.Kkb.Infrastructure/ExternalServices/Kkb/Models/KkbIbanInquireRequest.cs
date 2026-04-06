using LinkPara.Security.Helpers;
using System.Security;
using System.Text;


namespace LinkPara.Kkb.Infrastructure.ExternalServices.Kkb.Models;

public static class KkbIbanInquireRequest
{
    public static string CreateRequestXmlForIbanInquire(string userName, SecureString password, string iban)
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
        result.Append("<wsv2:yeniBsgIbanSorgulama>");
        result.Append("<IbanSorgulamaInputBean>");
        result.Append("<clientIp></clientIp>");
        result.Append("<kanal>7</kanal>");
        result.Append("<kullanici></kullanici>");
        result.Append($"<ibanNo>{iban}</ibanNo>");
        result.Append("<kullaniciAdSoyad></kullaniciAdSoyad>");
        result.Append("<kullaniciKodu></kullaniciKodu>");
        result.Append("<subeBayiAdi></subeBayiAdi>");
        result.Append("<subeBayiKodu></subeBayiKodu>");
        result.Append("</IbanSorgulamaInputBean>");
        result.Append("</wsv2:yeniBsgIbanSorgulama>");
        result.Append("</soapenv:Body>");
        result.Append("</soapenv:Envelope>");
        return result.ToString();
    }
}
