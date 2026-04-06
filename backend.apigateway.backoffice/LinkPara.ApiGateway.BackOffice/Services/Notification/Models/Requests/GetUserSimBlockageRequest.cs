using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class GetUserSimBlockageRequest : SearchQueryParams
{
    public DateTime? CreateDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string PhoneNumber { get; set; }
    public bool? IsSendOtp { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}
