using System.Globalization;
using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Response;

public class NestPayPointInquiryResponse : NestPayResponseBase
{
    public string DetailMessage { get; set; }
    public decimal AvailablePoint { get; set; }
    public NestPayPointInquiryResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        AvailablePoint = 0;

        var isBankpoint = responseXml.Descendants().FirstOrDefault(node => node.Name == "MAXIPUAN")?.Value;
        if (isBankpoint is not null)
        {
            AvailablePoint = Decimal.Parse(isBankpoint, CultureInfo.InvariantCulture);
        }
        var ziraatBankPoint = responseXml.Descendants().FirstOrDefault(node => node.Name == "KULLANILABILIRPUAN")?.Value;
        if (ziraatBankPoint is not null)
        {
            AvailablePoint = Decimal.Parse(ziraatBankPoint, CultureInfo.InvariantCulture);
        }

        ErrorMessage = responseXml.Element("ErrMsg")?.Value;
        OrderId = responseXml.Element("OrderId")?.Value;
        ResultCode = responseXml.Element("ProcReturnCode")?.Value;
        ResultDetail = responseXml.Element("Response")?.Value;
        AuthCode = responseXml.Element("AuthCode")?.Value;
        TransactionId = responseXml.Element("TransId")?.Value;
        ErrorCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "ERRORCODE")?.Value;
        DetailMessage = responseXml.Descendants().FirstOrDefault(node => node.Name == "HOSTMSG")?.Value;

        return this;
    }
}
