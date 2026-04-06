using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantApiValidationLog : AuditEntity
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public TransactionType TransactionType { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string CardToken { get; set; }
    public string Currency { get; set; }
    public int InstallmentCount { get; set; }
    public string ThreeDSessionId { get; set; }
    public string ConversationId { get; set; }
    public string OriginalReferenceNumber { get; set; }
    public string ClientIpAddress { get; set; }
    public string LanguageCode { get; set; }
    public string ApiName { get; set; }
}