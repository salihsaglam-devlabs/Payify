using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Response;

public class VakifPaymentDetailResponse : VakifResponseBase
{
    public string Status { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public string TransactionType { get; set; }
    public int CurrencyCode { get; set; }
    public int TotalItemCount { get; set; }
    public string Amount { get; set; }
    public string TotalRefundAmount { get; set; }
    public bool IsCanceled { get; set; }
    public bool IsReversed { get; set; }
    public bool IsRefunded { get; set; }
    public bool IsCaptured { get; set; }

    public VakifPaymentDetailResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        TransactionId = responseXml.Descendants().FirstOrDefault(node => node.Name == "TransactionId")?.Value;
        ResultCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "ResultCode")?.Value;
        AuthCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "AuthCode")?.Value;
        Rrn = responseXml.Descendants().FirstOrDefault(node => node.Name == "Rrn")?.Value;
        Status = responseXml.Descendants().FirstOrDefault(node => node.Name == "Status")?.Value;
        ResponseCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "ResponseCode")?.Value;
        ResponseMessage = responseXml.Descendants().FirstOrDefault(node => node.Name == "ResponseMessage")?.Value;
        TransactionType = responseXml.Descendants().FirstOrDefault(node => node.Name == "TransactionType")?.Value;
        CurrencyCode = Convert.ToInt32(responseXml.Descendants().FirstOrDefault(node => node.Name == "AmountCode")?.Value);
        TotalItemCount = Convert.ToInt32(responseXml.Descendants().FirstOrDefault(node => node.Name == "TotalItemCount")?.Value);
        Amount = responseXml.Descendants().FirstOrDefault(node => node.Name == "Amount")?.Value;
        TotalRefundAmount = responseXml.Descendants().FirstOrDefault(node => node.Name == "TotalRefundAmount")?.Value;
        IsCanceled = Convert.ToBoolean(responseXml.Descendants().FirstOrDefault(node => node.Name == "IsCanceled")?.Value);
        IsReversed = Convert.ToBoolean(responseXml.Descendants().FirstOrDefault(node => node.Name == "IsReversed")?.Value);
        IsRefunded = Convert.ToBoolean(responseXml.Descendants().FirstOrDefault(node => node.Name == "IsRefunded")?.Value);
        IsCaptured = Convert.ToBoolean(responseXml.Descendants().FirstOrDefault(node => node.Name == "IsCaptured")?.Value);

        if (!string.IsNullOrEmpty(responseXml.Descendants().FirstOrDefault(node => node.Name == "HostDate")?.Value))
            HostDate = Convert.ToDateTime(
                responseXml.Descendants().FirstOrDefault(node => node.Name == "HostDate")?.Value.Substring(0, 4) + '-' +
                responseXml.Descendants().FirstOrDefault(node => node.Name == "HostDate")?.Value.Substring(4, 2) + '-' +
                responseXml.Descendants().FirstOrDefault(node => node.Name == "HostDate")?.Value.Substring(6, 2));

        return this;
    }
}
