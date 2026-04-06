using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllMerchantTransactionRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public int? AcquireBankCode { get; set; }
    public int? IssuerBankCode { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public TimeoutTransactionType? TransactionType { get; set; }
    public PfTransactionStatus? TransactionStatus { get; set; }
    public PfTransactionSource? PfTransactionSource { get; set; }
    public string ConversationId { get; set; }
    public string BankOrderId { get; set; }
    public string CardFirstNumbers { get; set; }
    public string CardLastNumbers { get; set; }
    public bool? IsChargeBack { get; set; }
    public bool? IsManualReturn { get; set; }
    public bool? IsSuspecious { get; set; }
    public bool? IsOnUsPayment { get; set; }
    public bool? IsInsurancePayment { get; set; }
    public string CreatedNameBy { get; set; }
    public BlockageStatus? BlockageStatus { get; set; }
}