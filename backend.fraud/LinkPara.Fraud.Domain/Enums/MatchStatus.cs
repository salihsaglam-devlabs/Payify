namespace LinkPara.Fraud.Domain.Enums;

public enum MatchStatus
{
    Unknown,
    NoMatch,
    PotentialMatch,
    FalsePositive,
    TruePositive,
    TruePositiveApprove,
    TruePositiveReject
}
