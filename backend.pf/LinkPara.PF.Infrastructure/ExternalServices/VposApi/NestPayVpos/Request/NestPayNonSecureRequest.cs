using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Request;

public class NestPayNonSecureRequest : NestPayBaseRequest
{
    public int? BlockageCode { get; set; }
    private string BuildRequestCommon()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<CC5Request>");
        requestXml.AppendLine($"<Name>{MerchantName}</Name>");
        requestXml.AppendLine($"<Password>{Password}</Password>");
        requestXml.AppendLine($"<ClientId>{ClientId}</ClientId>");
        requestXml.AppendLine($"<IPAddress>{ClientIp}</IPAddress>");
        requestXml.AppendLine($"<OrderId>{OrderId}</OrderId>");
        requestXml.AppendLine($"<Type>{Type}</Type>");
        requestXml.AppendLine($"<Number>{Pan}</Number>");
        requestXml.AppendLine($"<Expires>{Expiry}</Expires>");
        requestXml.AppendLine($"<Cvv2Val>{Cvv}</Cvv2Val>");
        if (NumberOfInstallments > 1)
        {
            requestXml.AppendLine($"<Instalment>{NumberOfInstallments}</Instalment>");
        }
        requestXml.AppendLine($"<Total>{Amount}</Total>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine("<Extra>");
        requestXml.AppendLine($"<MAXIPUAN>{BonusAmount}</MAXIPUAN>");
        requestXml.AppendLine($"<SUBMERCHANTID>{SubMerchantId}</SUBMERCHANTID>");
        requestXml.AppendLine($"<SUBMERCHANTPOSTALCODE>{SubMerchantPostalCode}</SUBMERCHANTPOSTALCODE>");
        requestXml.AppendLine($"<SUBMERCHANTCITY>{SubMerchantCity}</SUBMERCHANTCITY>");
        requestXml.AppendLine($"<SUBMERCHANTCOUNTRY>{SubMerchantCountry}</SUBMERCHANTCOUNTRY>");
        requestXml.AppendLine($"<SUBMERCHANTMCC>{SubMerchantMcc}</SUBMERCHANTMCC>");

        return requestXml.ToString();
    }

    public string BuildRequest()
    {
        var commonRequestXml = BuildRequestCommon(); 

        var requestXml = new StringBuilder(commonRequestXml);
        if (IsBlockaged == true)
        {
            requestXml.AppendLine($"<BOLUM>{BlockageCode}</BOLUM>");
        }
        requestXml.AppendLine($"<SUBMERCHANTNAME>{SubMerchantName}</SUBMERCHANTNAME>");
        requestXml.AppendLine($"<SUBMERCHANTNIN>{SubMerchantGlobalMerchantId}</SUBMERCHANTNIN>");
        requestXml.AppendLine($"<SUBMERCHANTURL>{SubMerchantUrl}</SUBMERCHANTURL>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");

        return requestXml.ToString();
    }
    public string BuildRequestHalkbank()
    {
        var commonRequestXml = BuildRequestCommon();
        if (SubMerchantName.Length > 20)
        {
            SubMerchantName = SubMerchantName.Substring(0, 20);
        }
        var requestXml = new StringBuilder(commonRequestXml);
        requestXml.AppendLine($"<SUBMERCHANTNAME>{SubMerchantName}</SUBMERCHANTNAME>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");

        return requestXml.ToString();
    }
    public string BuildRequestAkbank()
    {
        var commonRequestXml = BuildRequestCommon();
        if (SubMerchantName.Length > 20)
        {
            SubMerchantName = SubMerchantName.Substring(0, 20);
        }
        var requestXml = new StringBuilder(commonRequestXml);
        requestXml.AppendLine($"<SUBMERCHANTNAME>{SubMerchantName}</SUBMERCHANTNAME>");
        requestXml.AppendLine($"<VISASUBMERCHANTID>{VisaSubmerchantId}</VISASUBMERCHANTID>");
        requestXml.AppendLine($"<VISAPFID>{VisaPfId}</VISAPFID>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");

        return requestXml.ToString();
    }
    public string BuildRequestSekerbank()
    {
        var commonRequestXml = BuildRequestCommon();
        if (SubMerchantName.Length > 25)
        {
            SubMerchantName = SubMerchantName.Substring(0, 25);
        }
        var requestXml = new StringBuilder(commonRequestXml);
        requestXml.AppendLine($"<SUBMERCHANTNAME>{SubMerchantName}</SUBMERCHANTNAME>");
        requestXml.AppendLine($"<SUBMERCHANTNUMBER>{SubMerchantNumber}</SUBMERCHANTNUMBER>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");

        return requestXml.ToString();
    }
    public string BuildRequestTfkb()
    {
        var commonRequestXml = BuildRequestCommon();
        if (SubMerchantName.Length > 25)
        {
            SubMerchantName = SubMerchantName.Substring(0, 25);
        }
        var requestXml = new StringBuilder(commonRequestXml);
        requestXml.AppendLine($"<SUBMERCHANTNAME>{SubMerchantName}</SUBMERCHANTNAME>");
        requestXml.AppendLine($"<SUBMERCHANTFACILITATORID>{VisaSubmerchantId}</SUBMERCHANTFACILITATORID>");
        requestXml.AppendLine($"<SUBMERCHANTTCKNVKN>{SubMerchantTaxNumber}</SUBMERCHANTTCKNVKN>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");

        return requestXml.ToString();
    }
    public string BuildRequestZiraat()
    {
        var requestXml = new StringBuilder();

        if (SubMerchantName?.Length > 14)
        {
            SubMerchantName = SubMerchantName.Substring(0, 14);
        }

        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<CC5Request>");
        requestXml.AppendLine($"<Name>{MerchantName}</Name>");
        requestXml.AppendLine($"<Password>{Password}</Password>");
        requestXml.AppendLine($"<ClientId>{ClientId}</ClientId>");
        requestXml.AppendLine($"<IPAddress>{ClientIp}</IPAddress>");
        requestXml.AppendLine($"<OrderId>{OrderId}</OrderId>");
        requestXml.AppendLine($"<Type>{Type}</Type>");
        requestXml.AppendLine($"<Number>{Pan}</Number>");
        requestXml.AppendLine($"<Expires>{Expiry}</Expires>");
        requestXml.AppendLine($"<Cvv2Val>{Cvv}</Cvv2Val>");
        if (NumberOfInstallments > 1)
        {
            requestXml.AppendLine($"<Instalment>{NumberOfInstallments}</Instalment>");
        }
        requestXml.AppendLine($"<Total>{Amount}</Total>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine("<Extra>");
        requestXml.AppendLine($"<MAXIPUAN>{BonusAmount}</MAXIPUAN>");
        requestXml.AppendLine($"<SUBMERCHANTNUMBER>{SubMerchantId}</SUBMERCHANTNUMBER>");
        requestXml.AppendLine($"<SUBMERCHANTNAME></SUBMERCHANTNAME>");
        requestXml.AppendLine($"<SUBMERCHANTMCC>{SubMerchantMcc}</SUBMERCHANTMCC>");
        requestXml.AppendLine($"<SUBMERCHANTID>{SubMerchantName}</SUBMERCHANTID>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");

        return requestXml.ToString();
    }

    public string BuildRequestAnadoluBank()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<CC5Request>");
        requestXml.AppendLine($"<Name>{MerchantName}</Name>");
        requestXml.AppendLine($"<Password>{Password}</Password>");
        requestXml.AppendLine($"<ClientId>{ClientId}</ClientId>");
        requestXml.AppendLine($"<IPAddress>{ClientIp}</IPAddress>");
        requestXml.AppendLine($"<OrderId>{OrderId}</OrderId>");
        requestXml.AppendLine($"<Type>{Type}</Type>");
        requestXml.AppendLine($"<Number>{Pan}</Number>");
        requestXml.AppendLine($"<Expires>{Expiry}</Expires>");
        requestXml.AppendLine($"<Cvv2Val>{Cvv}</Cvv2Val>");
        if (NumberOfInstallments > 1)
        {
            requestXml.AppendLine($"<Instalment>{NumberOfInstallments}</Instalment>");
        }
        requestXml.AppendLine($"<Total>{Amount}</Total>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine("<Extra>");
        requestXml.AppendLine($"<MAXIPUAN>{BonusAmount}</MAXIPUAN>");
        requestXml.AppendLine($"<SUBMERCHANTID>{SubMerchantTaxNumber}</SUBMERCHANTID>");
        requestXml.AppendLine($"<SUBMERCHANTPOSTALCODE>{SubMerchantPostalCode}</SUBMERCHANTPOSTALCODE>");
        requestXml.AppendLine($"<SUBMERCHANTCITY>{SubMerchantCity}</SUBMERCHANTCITY>");
        requestXml.AppendLine($"<SUBMERCHANTCOUNTRY>{SubMerchantCountry}</SUBMERCHANTCOUNTRY>");
        requestXml.AppendLine($"<SUBMERCHANTMCC>{SubMerchantMcc}</SUBMERCHANTMCC>");
        requestXml.AppendLine($"<SUBMERCHANTNAME>{SubMerchantName}</SUBMERCHANTNAME>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");

        return requestXml.ToString();
    }
}
