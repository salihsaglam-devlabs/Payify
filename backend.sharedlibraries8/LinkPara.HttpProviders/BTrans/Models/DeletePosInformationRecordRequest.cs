namespace LinkPara.HttpProviders.BTrans.Models;

public class DeletePosInformationRecordRequest
{
    public Guid RelatedTransactionId { get; set; }
    public Guid RelatedInstallmentTransactionId { get; set; }
    public string RecordType { get; set; }
}