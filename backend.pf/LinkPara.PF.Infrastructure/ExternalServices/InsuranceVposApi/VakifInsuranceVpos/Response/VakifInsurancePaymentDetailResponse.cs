using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Response;

public class VakifInsurancePaymentDetailResponse : VakifInsuranceResponseBase
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
    
    public VakifInsurancePaymentDetailResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        TransactionId = responseXml.Descendants("TransactionId").FirstOrDefault()?.Value;
        ResultCode = responseXml.Descendants("ResultCode").FirstOrDefault()?.Value;
        AuthCode = responseXml.Descendants("AuthCode").FirstOrDefault()?.Value;
        Rrn = responseXml.Descendants("Rrn").FirstOrDefault()?.Value;
        Status = responseXml.Descendants("Status").FirstOrDefault()?.Value;
        ResponseCode = responseXml.Descendants("ResponseCode").FirstOrDefault()?.Value;
        ResponseMessage = responseXml.Descendants("ResponseMessage").FirstOrDefault()?.Value;
        TransactionType = responseXml.Descendants( "TransactionType").FirstOrDefault()?.Value;
        CurrencyCode = Convert.ToInt32(responseXml.Descendants("AmountCode").FirstOrDefault()?.Value);
        TotalItemCount = Convert.ToInt32(responseXml.Descendants("TotalItemCount").FirstOrDefault()?.Value);
        Amount = responseXml.Descendants("Amount").FirstOrDefault()?.Value;
        TotalRefundAmount = responseXml.Descendants("TotalRefundAmount").FirstOrDefault()?.Value;
        IsCanceled = Convert.ToBoolean(responseXml.Descendants("IsCanceled").FirstOrDefault()?.Value);
        IsReversed = Convert.ToBoolean(responseXml.Descendants("IsReversed").FirstOrDefault()?.Value);
        IsRefunded = Convert.ToBoolean(responseXml.Descendants("IsRefunded").FirstOrDefault()?.Value);
        IsCaptured = Convert.ToBoolean(responseXml.Descendants("IsCaptured").FirstOrDefault()?.Value);

        var hostDateRaw = responseXml.Descendants("HostDate").FirstOrDefault()?.Value;

        if (!string.IsNullOrEmpty(hostDateRaw) && hostDateRaw.Length >= 8)
        {
            HostDate = hostDateRaw;
        }
        return this;
    }
}