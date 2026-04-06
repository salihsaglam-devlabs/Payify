using LinkPara.ApiGateway.Services.Fraud.Models.Enums;
using LinkPara.HttpProviders.Fraud.Models.Enums;

namespace LinkPara.ApiGateway.Services.Fraud.Models.Response
{
    public class TransactionMonitoringResponse
    {
        public string Module { get; set; }
        public string CommandName { get; set; }
        public string SenderName { get; set; }
        public string SenderNumber { get; set; }
        public string ReceiverNumber { get; set; }
        public string ReceiverName { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string TransactionId { get; set; }
        public int TotalScore { get; set; }
        public MonitoringStatus MonitoringStatus { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ClientIpAddress { get; set; }
    }
}
