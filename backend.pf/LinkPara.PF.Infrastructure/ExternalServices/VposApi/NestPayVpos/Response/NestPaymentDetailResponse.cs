using System.Globalization;
using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Response;

public class NestPaymentDetailResponse : NestPayResponseBase
{
    public string TransactionStatus { get; set; }
    public string ChargeType { get; set; }
    public string Amount { get; set; }
    public NestPaymentDetailResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        ResultCode = responseXml.Element("ProcReturnCode")?.Value;
        ErrorMessage = responseXml.Element("ErrMsg")?.Value;
        ResultDetail = responseXml.Element("Response")?.Value;
        OrderId = responseXml.Element("OrderId")?.Value;
        TransactionId = responseXml.Element("TransId")?.Value;
        AuthCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "AUTH_CODE")?.Value;
        Amount = responseXml.Descendants().FirstOrDefault(node => node.Name == "CAPTURE_AMT")?.Value;
        TransactionStatus = responseXml.Descendants().FirstOrDefault(node => node.Name == "TRANS_STAT")?.Value;
        ChargeType = responseXml.Descendants().FirstOrDefault(node => node.Name == "CHARGE_TYPE_CD")?.Value;

        if (!string.IsNullOrEmpty(responseXml.Descendants().FirstOrDefault(node => node.Name == "AUTH_DTTM")?.Value))
            TrxDate = ConvertTranDateToDatetime(responseXml.Descendants().FirstOrDefault(node => node.Name == "AUTH_DTTM")?.Value);

        return this;
    }

    private DateTime ConvertTranDateToDatetime(string tranDate)
    {
        const string dateFormat = "yyyy-MM-dd HH:mm:ss.f";

        if (DateTime.TryParseExact(tranDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return date;
        }

        return DateTime.Now;
    }
}
