using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class GetCustomerAccountInfoRequest : SearchQueryParams
{
    public Guid AccountId { get; set; }
    public Guid ConfirmationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TransactionDirection? TransactionDirection { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string TransactionTypes { get; set; }
    public TransactionStatus? TransactionStatus { get; set; }

}