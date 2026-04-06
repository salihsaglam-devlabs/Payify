using LinkPara.ApiGateway.CorporateWallet.Commons.Helpers;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;
using LinkPara.ApiGateway.CorporateWallet.Services.Identity.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;

public class CreateTransferOrderRequest
{
    public string SenderWalletNumber { get; set; }
    public string SenderNameSurname { get; set; }
    public UserType SenderUserType { get; set; }
    public ReceiverAccountType ReceiverAccountType { get; set; }
    public string ReceiverAccountValue { get; set; }
    public string ReceiverNameSurname { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; }
    public string ReceiverPhoneCode { get; set; }
}

public class CreateTransferOrderServiceRequest : CreateTransferOrderRequest, IHasUserId
{
    public Guid UserId { get; set; }
}
