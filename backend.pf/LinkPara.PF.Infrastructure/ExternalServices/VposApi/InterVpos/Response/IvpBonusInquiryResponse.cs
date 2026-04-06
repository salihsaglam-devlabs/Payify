using System.Globalization;
using System.Web;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Response;

public class IvpBonusInquiryResponse : IvpResponseBase
{
    public string OrderId { get; set; }
    public decimal EarnedBonus { get; set; }
    public decimal UsedBonus { get; set; }
    public decimal AvailableBonus { get; set; }
    public decimal BonusToBonus { get; set; }
    public decimal FoldedBonus { get; set; }

    public IvpBonusInquiryResponse Parse(string response)
    {
        response = response.Replace(";;", ";").Replace(";", "&");

        var responseParams = HttpUtility.ParseQueryString(response);

        TransId = responseParams["TransId"];
        ProcReturnCode = responseParams["ProcReturnCode"];
        HostRefNum = responseParams["HostRefNum"];
        AuthCode = responseParams["AuthCode"];
        TxnResult = responseParams["TxnResult"];
        ErrorMessage = responseParams["ErrorMessage"];
        OrderId = responseParams["OrderId"];
        EarnedBonus = decimal.Parse(responseParams["EarnedBonus"].Replace(",", "."), CultureInfo.InvariantCulture);
        AvailableBonus = decimal.Parse(responseParams["AvailableBonus"].Replace(",", "."), CultureInfo.InvariantCulture);
        BonusToBonus = decimal.Parse(responseParams["BonusToBonus"].Replace(",", "."), CultureInfo.InvariantCulture);
        FoldedBonus = decimal.Parse(responseParams["FoldedBonus"].Replace(",", "."), CultureInfo.InvariantCulture);

        return this;
    }
}