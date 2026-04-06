using System.Globalization;
using System.Xml.Linq;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Response;
public class FinansPointInquiryResponse : FinansResponseBase
{
    public decimal AvailablePoint { get; set; }
    public decimal AvailableBonusPoint {  get; set; }
    public decimal AvailableTravelAdvanceAmount { get; set; }
    public DateTime TravelAdvancePointExpiryDate { get; set; }
    public FinansPointInquiryResponse Parse(string response)
    {
        var responseXml = XElement.Parse(response);

        ProcReturnCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "ProcReturnCode")?.Value;
        HostRefNum = responseXml.Descendants().FirstOrDefault(node => node.Name == "HostRefNum")?.Value;
        AuthCode = responseXml.Descendants().FirstOrDefault(node => node.Name == "AuthCode")?.Value;
        TxnResult = responseXml.Descendants().FirstOrDefault(node => node.Name == "TxnResult")?.Value;
        ErrorMessage = responseXml.Descendants().FirstOrDefault(node => node.Name == "ErrorMessage")?.Value;
        OrderId = responseXml.Descendants().FirstOrDefault(node => node.Name == "OrderId")?.Value;
        AvailablePoint = decimal.Parse(responseXml.Descendants().FirstOrDefault(node => node.Name == "AvailablePoint")?.Value, 
            CultureInfo.InvariantCulture);
        AvailableBonusPoint = decimal.Parse(responseXml.Descendants().FirstOrDefault(node => node.Name == "KullanilabilirBonusPuan")?.Value, 
            CultureInfo.InvariantCulture);
        AvailableTravelAdvanceAmount = decimal.Parse(responseXml.Descendants().FirstOrDefault(node => node.Name == "KullanilabilirSeyahatAvansTutar")?.Value, 
            CultureInfo.InvariantCulture);
        TravelAdvancePointExpiryDate = DateTime.Parse(responseXml.Descendants().FirstOrDefault(node => node.Name == "SeyahatAvansPuanVadeTarihi")?.Value, 
            CultureInfo.InvariantCulture);

        return this;
    }
}
