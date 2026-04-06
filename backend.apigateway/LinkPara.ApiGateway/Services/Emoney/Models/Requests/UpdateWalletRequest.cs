using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class UpdateWalletRequest
{
    public Guid WalletId { get; set; }
    public string FriendlyName { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public bool IsBlocked { get; set; }
}

public class UpdateWalletServiceRequest : UpdateWalletRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
