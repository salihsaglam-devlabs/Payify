using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Response;
public class FinansResponseBase
{
    public string OrderId { get; set; }
    public string TxnResult { get; set; }
    public string ErrorMessage { get; set; }
    public string ProcReturnCode { get; set; }
    public string AuthCode { get; set; }
    public string HostRefNum { get; set; }

    public static DateTime TranDateToDatetime(string tranDate)
    {
        string format = "dd.MM.yyyy HH:mm";
        DateTime dateTime;

        if (DateTime.TryParseExact(tranDate, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
        {
            return dateTime;
        }

        return DateTime.Now;
    }
}
