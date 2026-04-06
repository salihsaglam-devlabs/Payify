using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.ApiGateway.Services.Identity.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class TransferOrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string SenderWalletNumber { get; set; }
    public string SenderNameSurname { get; set; }
    public UserType SenderUserType { get; set; }
    public ReceiverAccountType ReceiverAccountType { get; set; }
    public string ReceiverAccountValue { get; set; }
    public string ReceiverNameSurname { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; }
    public TransferOrderStatus TransferOrderStatus { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string ReceiverPhoneCode { get; set; }
    public string CurrencyCode { get; set; }
    public CurrencyDto Currency { get; set; }
}