using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Response;

public class PosnetPaymentNonSecureResponse : PosnetResponseBase
{
    public PosnetPaymentNonSecureResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        Approved = responseXml.Element("approved")?.Value;
        ErrorCode = responseXml.Element("respCode")?.Value;
        ErrorMessage = responseXml.Element("respText")?.Value;
        HostLogKey = responseXml.Element("hostlogkey")?.Value;
        AuthCode = responseXml.Element("authCode")?.Value;
        InstallmentCount = responseXml.Descendants().FirstOrDefault(node => node.Name == "inst1")?.Value;
        InstAmount = responseXml.Descendants().FirstOrDefault(node => node.Name == "amnt1")?.Value;
       
        if (!string.IsNullOrEmpty(responseXml.Element("tranDate")?.Value))
            TranDate = TranDateToDatetime(responseXml.Element("tranDate")?.Value);

        return this;
    }
}
