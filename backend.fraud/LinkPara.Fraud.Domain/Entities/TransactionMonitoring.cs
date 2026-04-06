using LinkPara.Fraud.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Fraud.Domain.Entities;

public class TransactionMonitoring : AuditEntity
{
    public string Module { get; set; }
    public string CommandName { get; set; }
    public string TransferRequest { get; set; }
    public string CommandJson { get; set; }
    public string SenderNumber { get; set; }
    public string SenderName { get; set; }
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
