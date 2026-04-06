using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Request;

public class NestPayRefundRequest : NestPayBase
{
    public string OrderId { get; set; }
    public string Type { get; set; }
    public string Amount { get; set; }
    public int CurrencyCode { get; set; }
    public string SubMerchantName { get; set; }
    public string SubMerchantId { get; set; }
    public string SubMerchantNumber { get; set; }
    public string SubMerchantPostalCode { get; set; }
    public string SubMerchantCity { get; set; }
    public string SubMerchantCountry { get; set; }
    public string SubMerchantMcc { get; set; }
    public string SubMerchantGlobalMerchantId { get; set; }
    public string SubMerchantUrl { get; set; }
    public string SubMerchantTaxNumber { get; set; }
    public string VisaSubmerchantId { get; set; }
    public string VisaPfId { get; set; }
    public string ClientIp { get; set; }
    private string BuildRequestCommon()
    {
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<CC5Request>");
        requestXml.AppendLine($"<Name>{MerchantName}</Name>");
        requestXml.AppendLine($"<Password>{Password}</Password>");
        requestXml.AppendLine($"<ClientId>{ClientId}</ClientId>");
        requestXml.AppendLine($"<OrderId>{OrderId}</OrderId>");
        requestXml.AppendLine($"<Type>{Type}</Type>");
        requestXml.AppendLine($"<Total>{Amount}</Total>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine("<Extra>");
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
        if (SubMerchantName.Length > 20)
        {
            SubMerchantName = SubMerchantName.Substring(0, 20);
        }
        var requestXml = new StringBuilder(commonRequestXml);
        requestXml.AppendLine($"<SUBMERCHANTNAME>{SubMerchantName}</SUBMERCHANTNAME>");
        requestXml.AppendLine($"<SUBMERCHANTNIN>{SubMerchantGlobalMerchantId}</SUBMERCHANTNIN>");
        requestXml.AppendLine($"<SUBMERCHANTURL>{SubMerchantUrl}</SUBMERCHANTURL>");
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
        if (SubMerchantName.Length > 25)
        {
            SubMerchantName = SubMerchantName.Substring(0, 25);
        }
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<CC5Request>");
        requestXml.AppendLine($"<Name>{MerchantName}</Name>");
        requestXml.AppendLine($"<Password>{Password}</Password>");
        requestXml.AppendLine($"<ClientId>{ClientId}</ClientId>");
        requestXml.AppendLine($"<OrderId>{OrderId}</OrderId>");
        requestXml.AppendLine($"<Type>{Type}</Type>");
        requestXml.AppendLine($"<Total>{Amount}</Total>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine("<Extra>");
        requestXml.AppendLine($"<SUBMERCHANTID>{SubMerchantTaxNumber}</SUBMERCHANTID>");
        requestXml.AppendLine($"<SUBMERCHANTPOSTALCODE>{SubMerchantPostalCode}</SUBMERCHANTPOSTALCODE>");
        requestXml.AppendLine($"<SUBMERCHANTCITY>{SubMerchantCity}</SUBMERCHANTCITY>");
        requestXml.AppendLine($"<SUBMERCHANTCOUNTRY>{SubMerchantCountry}</SUBMERCHANTCOUNTRY>");
        requestXml.AppendLine($"<SUBMERCHANTMCC>{SubMerchantMcc}</SUBMERCHANTMCC>");
        requestXml.AppendLine($"<SUBMERCHANTNAME>{SubMerchantName}</SUBMERCHANTNAME>");
        requestXml.AppendLine($"<SUBMERCHANTFACILITATORID>{VisaSubmerchantId}</SUBMERCHANTFACILITATORID>");
        requestXml.AppendLine($"<SUBMERCHANTTCKNVKN>{SubMerchantTaxNumber}</SUBMERCHANTTCKNVKN>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");

        return requestXml.ToString();
    }
    public string BuildRequestZiraat()
    {
        if (SubMerchantName?.Length > 14)
        {
            SubMerchantName = SubMerchantName.Substring(0, 14);
        }
        var requestXml = new StringBuilder();
        requestXml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        requestXml.AppendLine("<CC5Request>");
        requestXml.AppendLine($"<Name>{MerchantName}</Name>");
        requestXml.AppendLine($"<Password>{Password}</Password>");
        requestXml.AppendLine($"<ClientId>{ClientId}</ClientId>");
        requestXml.AppendLine($"<OrderId>{OrderId}</OrderId>");
        requestXml.AppendLine($"<Type>{Type}</Type>");
        requestXml.AppendLine($"<Total>{Amount}</Total>");
        requestXml.AppendLine($"<Currency>{CurrencyCode}</Currency>");
        requestXml.AppendLine("<Extra>");
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
        var commonRequestXml = BuildRequestCommon();
        
        var requestXml = new StringBuilder(commonRequestXml);
        requestXml.AppendLine($"<SUBMERCHANTNAME>{SubMerchantName}</SUBMERCHANTNAME>");
        requestXml.AppendLine("</Extra>");
        requestXml.AppendLine("</CC5Request>");
        
        return requestXml.ToString();
    }
}
