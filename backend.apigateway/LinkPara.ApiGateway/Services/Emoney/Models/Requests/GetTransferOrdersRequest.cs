using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.ApiGateway.Services.Identity.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetTransferOrdersRequest : SearchQueryParams
{
    public ReceiverAccountType? ReceiverAccountType { get; set; }
    public TransferOrderStatus? TransferOrderStatus { get; set; }
    public string ReceiverAccountValue { get; set; }
    public string ReceiverNameSurname { get; set; }
    public string SenderWalletNumber { get; set; }
    public string SenderNameSurname { get; set; }
    public UserType? SenderUserType { get; set; }
    public DateTime? TransferDateStart { get; set; }
    public DateTime? TransferDateEnd { get; set; }
    public Guid? UserId { get; set; }
}
