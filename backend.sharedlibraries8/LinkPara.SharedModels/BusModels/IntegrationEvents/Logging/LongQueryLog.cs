using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Logging
{
    public class LongQueryLog
    {
        public string CorrelationId { get; set; }
        public string DatabaseName { get; set; }
        public string CommandText { get; set; }
        public List<string> Parameters{ get; set; }
        public DateTime Date { get; set; }
        public decimal DurationInMilliseconds { get; set; }
        public bool HasErrors{ get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public IntegrationLogDataType DataType { get; set; }
    }
}
