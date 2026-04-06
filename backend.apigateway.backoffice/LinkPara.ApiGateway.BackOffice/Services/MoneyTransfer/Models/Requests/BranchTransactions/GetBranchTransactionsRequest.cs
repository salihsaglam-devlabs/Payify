using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Requests.BranchTransactions;

public class GetBranchTransactionsRequest : SearchQueryParams
{
    public Guid? SenderBranchId { get; set; }
    public string SenderFirstName { get; set; }
    public string SenderLastName { get; set; }
    public string BeneficieryFirstName { get; set; }
    public string BeneficieryLastName { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}