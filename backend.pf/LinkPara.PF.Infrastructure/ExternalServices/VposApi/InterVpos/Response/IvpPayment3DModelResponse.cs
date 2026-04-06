using System.Globalization;
using System.Web;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Response;

public class IvpPayment3DModelResponse : IvpResponseBase
{
    public string OrderId { get; set; }

    public IvpPayment3DModelResponse Parse(string response)
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

        if (!string.IsNullOrEmpty(responseParams["TrxDate"]))
            TrxDate = DateTime.ParseExact(responseParams["TrxDate"], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

        return this;
    }
}