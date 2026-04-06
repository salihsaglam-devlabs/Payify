using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Logging
{
    public class IntegrationLog
    {
        public string CorrelationId { get; set; }
        public string MethodName { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public DateTime Date { get; set; }
        public string HttpCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public IntegrationLogDataType DataType { get; set; }
    }
}
