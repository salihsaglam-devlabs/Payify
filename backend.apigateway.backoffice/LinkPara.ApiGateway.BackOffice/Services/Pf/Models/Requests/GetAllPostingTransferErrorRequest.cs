using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetAllPostingTransferErrorRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public Guid? MerchantTransactionId { get; set; }
    public DateTime? PostingDateStart { get; set; }
    public DateTime? PostingDateEnd { get; set; }
    public DateTime? TransactionStartDate { get; set; }
    public DateTime? TransactionEndDate { get; set; }
    public PostingTransferErrorCategory? TransferErrorCategory { get; set; }
    public string MerchantOrderId { get; set; }
}
