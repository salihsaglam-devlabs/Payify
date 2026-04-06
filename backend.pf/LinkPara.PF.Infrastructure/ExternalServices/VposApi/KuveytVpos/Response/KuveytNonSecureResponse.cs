using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Response;

public class KuveytNonSecureResponse : KuveytResponseBase
{
    public KuveytNonSecureResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        OrderId = responseXml.Element("OrderId")?.Value;
        ProvisionNumber = responseXml.Element("ProvisionNumber")?.Value;
        RRN = responseXml.Element("RRN")?.Value;
        ResponseCode = responseXml.Element("ResponseCode")?.Value;
        ResponseMessage = responseXml.Element("ResponseMessage")?.Value;
        Stan = responseXml.Element("Stan")?.Value;

        if (!string.IsNullOrEmpty(responseXml.Descendants().FirstOrDefault(node => node.Name == "TransactionTime")?.Value))
            TrxDate = TrxDateToDatetime(responseXml.Descendants().FirstOrDefault(node => node.Name == "TransactionTime")?.Value);

        return this;
    }
}
