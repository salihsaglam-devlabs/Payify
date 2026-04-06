using System.Globalization;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Response;

public class NestPayResponseBase
{
    public string MerchantId { get; set; }
    public string TransactionId { get; set; }
    public string GroupId { get; set; }
    public string OrderId { get; set; }
    public string ResultCode { get; set; }
    public string ResultDetail { get; set; }
    public string AuthCode { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime TrxDate { get; set; }

    public DateTime TrxDateToDatetime(string trxDate)
    {
        const string dateFormat = "yyyyMMddHHmmss";

        if (DateTime.TryParseExact(trxDate, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
        {
            return date;
        }

        return DateTime.Now;
    }
}
