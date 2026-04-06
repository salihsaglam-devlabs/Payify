namespace LinkPara.SharedModels.BusModels.Commands.BTrans;

public class DeletePosInformationRecord
{
    public Guid RelatedTransactionId { get; set; }
    public string RecordType { get; set; }
}