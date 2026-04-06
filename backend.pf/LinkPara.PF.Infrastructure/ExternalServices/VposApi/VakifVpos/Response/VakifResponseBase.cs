using System.Globalization;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Response;

public class VakifResponseBase
{
    public string MerchantId { get; set; }
    public string TransactionId { get; set; }
    public string ResultCode { get; set; }
    public string ResultDetail { get; set; }
    public string AuthCode { get; set; }
    public string Rrn { get; set; }
    public DateTime HostDate { get; set; }

    public DateTime HostDateToDatetime(string hostDate)
    {
        const string dateFormat = "yyyyMMddHHmmss";

        if (DateTime.TryParseExact(hostDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return date;
        }

        return DateTime.Now;
    }
}
