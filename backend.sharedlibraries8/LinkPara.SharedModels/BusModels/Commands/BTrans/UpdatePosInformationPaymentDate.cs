namespace LinkPara.SharedModels.BusModels.Commands.BTrans;

public class UpdatePosInformationPaymentDate
{
    public List<Guid> RelatedBalanceIds { get; set; }
    public DateTime PaymentDate { get; set; }
    public string ReceiverIban { get; set; }
    public string ReceiverBankName { get; set; }
    public string ReceiverBankCode { get; set; }
    public string ReceiverWalletNumber { get; set; }
    public string SenderBankName { get; set; }
    public string SenderBankCode { get; set; }
    public string SenderAccountNumber { get; set; }
    public string SenderIbanNumber { get; set; }
}