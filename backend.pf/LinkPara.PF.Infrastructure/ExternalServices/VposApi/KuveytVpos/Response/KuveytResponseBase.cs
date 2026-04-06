using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Response;

public class KuveytResponseBase
{
    public string OrderId { get; set; }
    public string ProvisionNumber { get; set; }
    public string RRN { get; set; }
    public string Stan { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
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
