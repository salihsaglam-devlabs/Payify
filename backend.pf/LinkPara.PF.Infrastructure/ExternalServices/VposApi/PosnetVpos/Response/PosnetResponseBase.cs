using System.Globalization;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Response
{
    public class PosnetResponseBase
    {
        public string Approved { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string HostLogKey { get; set; }
        public string OrderNumber { get; set; }
        public string AuthCode { get; set; }
        public DateTime TranDate { get; set; }
        public string InstallmentCount { get; set; }
        public string InstAmount { get; set; }
        public string Point { get; set; }
        public string PointAmount { get; set; }
        public string TotalPoint { get; set; }
        public string TotalPointAmount { get; set; }
        public string CurrencyCode { get; set; }

        public DateTime TranDateToDatetime(string tranDate)
        {
            const string dateFormat = "yyyyMMddHHmmss";

            string currentYear = DateTime.Today.Year.ToString();
            string fullTranDateStr = $"{currentYear.Substring(0,2)}{tranDate}";

            if (DateTime.TryParseExact(fullTranDateStr, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }

            return DateTime.Now;
        }
    }
}
