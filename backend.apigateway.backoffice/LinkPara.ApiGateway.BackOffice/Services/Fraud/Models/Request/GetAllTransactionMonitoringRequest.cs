using LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Enums;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Fraud.Models.Request
{
    public class GetAllTransactionMonitoringRequest : SearchQueryParams
    {
        public string CommandName { get; set; }
        public string Module { get; set; }
        public string SenderNumber { get; set; }
        public string ReceiverNumber { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public RiskLevel? RiskLevel { get; set; }
        public MonitoringStatus? MonitoringStatus { get; set; }
        public DateTime? TransactionDateStart { get; set; }
        public DateTime? TransactionDateEnd { get; set; }
    }
}