using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetFilterMerchantStatementRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public DateTime? StatementStartDate { get; set; }
    public DateTime? StatementEndDate { get; set; }
    public string MailAddress { get; set; }
    public int? StatementMonth { get; set; }
    public int? StatementYear { get; set; }
    public MerchantStatementStatus? StatementStatus { get; set; }
    public MerchantStatementType? StatementType { get; set; }
    public string ReceiptNumber { get; set; }
}
