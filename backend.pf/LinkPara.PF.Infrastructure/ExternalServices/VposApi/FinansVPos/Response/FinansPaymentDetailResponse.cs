using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Response;
public class FinansPaymentDetailResponse : FinansResponseBase
{
    public bool IsVoided { get; set; }
    public bool IsRefunded { get; set; }
    public string TxnType { get; set; }
    public string SecureType { get; set; }
    public DateTime TxnDate { get; set; }
    public FinansPaymentDetailResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        ProcReturnCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "ProcReturnCode")?.Value;
        HostRefNum = responseXml.Descendants().FirstOrDefault(node => node.Name == "HostRefNum")?.Value;
        AuthCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "AuthCode")?.Value;
        TxnResult = responseXml.Descendants().FirstOrDefault(node => node.Name == "TxnResult")?.Value;
        ErrorMessage = responseXml.Descendants().FirstOrDefault(node => node.Name == "ErrorMessage")?.Value;
        OrderId = responseXml.Descendants().FirstOrDefault(node => node.Name == "OrderId")?.Value;
        IsVoided = Convert.ToBoolean(responseXml.Descendants().FirstOrDefault(node => node.Name == "IsVoided")?.Value);
        IsRefunded = Convert.ToBoolean(responseXml.Descendants().FirstOrDefault(node => node.Name == "IsRefunded")?.Value);
        TxnType = responseXml.Descendants().FirstOrDefault(node => node.Name == "TxnType")?.Value;
        SecureType = responseXml.Descendants().FirstOrDefault(node => node.Name == "SecureType")?.Value;
        if (!string.IsNullOrEmpty(responseXml.Descendants().LastOrDefault(node => node.Name == "trxDate")?.Value))
            TxnDate = TranDateToDatetime(responseXml.Descendants().LastOrDefault(node => node.Name == "trxDate")?.Value);
        return this;
    }
}
