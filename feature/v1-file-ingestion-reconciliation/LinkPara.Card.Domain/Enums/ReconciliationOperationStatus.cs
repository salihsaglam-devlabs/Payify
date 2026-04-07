namespace LinkPara.Card.Domain.Enums;

public enum ReconciliationOperationStatus
{
    Pending = 0,
    Processing = 1,
    Blocked = 2,
    Done = 3,
    Failed = 4,
    Rejected = 5,
    Skipped = 6
}
