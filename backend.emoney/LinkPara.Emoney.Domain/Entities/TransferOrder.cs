using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Domain.Entities;

public class TransferOrder : AuditEntity
{
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
    public string ReceiverPhoneCode { get; set; }
    public string CurrencyCode { get; set; }
    public Currency Currency { get; set; }
    public string ErrorMessage { get; set; }
    public string PaymentType { get; set; }
}