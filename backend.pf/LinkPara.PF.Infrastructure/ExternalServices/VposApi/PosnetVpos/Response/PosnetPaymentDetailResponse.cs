using System.Globalization;
using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Response;

public class PosnetPaymentDetailResponse : PosnetResponseBase
{
    public string Amount { get; set; }
    public string CardNo { get; set; }
    public string Status { get; set; }
    public string TxnStatus { get; set; }
    public PosnetPaymentDetailResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        Approved = responseXml.Element("approved")?.Value;
        ErrorCode = responseXml.Element("respCode")?.Value;
        ErrorMessage = responseXml.Element("respText")?.Value;
        OrderNumber = responseXml.Descendants().LastOrDefault(node => node.Name == "orderID")?.Value;
        CardNo = responseXml.Descendants().LastOrDefault(node => node.Name == "ccno")?.Value;
        Amount = responseXml.Descendants().LastOrDefault(node => node.Name == "amount")?.Value;
        CurrencyCode = responseXml.Descendants().LastOrDefault(node => node.Name == "currencyCode")?.Value;
        AuthCode = responseXml.Descendants().LastOrDefault(node => node.Name == "authCode")?.Value;
        Status = responseXml.Descendants().LastOrDefault(node => node.Name == "state")?.Value;
        TxnStatus = responseXml.Descendants().LastOrDefault(node => node.Name == "txnStatus")?.Value;
        HostLogKey = responseXml.Descendants().LastOrDefault(node => node.Name == "hostLogKey")?.Value;
       
       
        if (!string.IsNullOrEmpty(responseXml.Descendants().LastOrDefault(node => node.Name == "tranDate")?.Value))
            TranDate = ConvertTranDateToDatetime(responseXml.Descendants().LastOrDefault(node => node.Name == "tranDate")?.Value);

        return this;
    }

    private DateTime ConvertTranDateToDatetime(string tranDate)
    {
        const string dateFormat = "yyyy-MM-dd HH:mm:ss.fff";

        if (DateTime.TryParseExact(tranDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return date;
        }

        return DateTime.Now;
    }
}
