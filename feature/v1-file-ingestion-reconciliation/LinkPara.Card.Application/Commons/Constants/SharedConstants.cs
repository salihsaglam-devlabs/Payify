namespace LinkPara.Card.Application.Commons.Constants;

public static class CorrelationKeyValues
{
    public const string CardPrefix = "CARD";
    public const string OceanTxnGuidPrefix = "OTG";
    public const string SegmentSeparator = ":";
    public const string DuplicateSignatureSeparator = "|";
}

public static class EMoneyLookupStates
{
    public const string Unavailable = "UNAVAILABLE";
    public const string EmptyKey = "EMPTY_KEY";
    public const string NotFound = "NOT_FOUND";
}

public static class ProcessLockNames
{
    public const string CardFileIngestion = "CARD_FILE_INGESTION";
    public const string CardReconciliation = "CARD_RECONCILIATION";
}
