using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Response;

public class NestPayRefundResponse : NestPayResponseBase
{
    public string ErrorDetailMessage { get; set; }
    public NestPayRefundResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        TransactionId = responseXml.Element("TransId")?.Value;
        OrderId = responseXml.Element("OrderId")?.Value;
        GroupId = responseXml.Element("GroupId")?.Value;
        MerchantId = responseXml.Descendants().FirstOrDefault(node => node.Name == "MERCHANTID")?.Value;
        ResultCode = responseXml.Element("ProcReturnCode")?.Value;
        AuthCode = responseXml.Element("AuthCode")?.Value;
        ResultDetail = responseXml.Element("Response")?.Value;
        ErrorMessage = responseXml.Element("ErrMsg")?.Value;
        ErrorCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "ERRORCODE")?.Value;
        ErrorDetailMessage = responseXml.Descendants().FirstOrDefault(node => node.Name == "HOSTMSG")?.Value;

        if (!string.IsNullOrEmpty(responseXml.Descendants().FirstOrDefault(node => node.Name == "TRXDATE")?.Value))
            TrxDate = TrxDateToDatetime(responseXml.Descendants().FirstOrDefault(node => node.Name == "TRXDATE")?.Value);

        return this;
    }
}
