namespace LinkPara.Card.Domain.Constants;

public static class ClearingLookupCodes
{
    public static class TxnType
    {
        public const string Issuer = "I";
        public const string Acquirer = "A";
        public const string Document = "D";
        public const string Fee = "F";
        public const string Fraud = "R";
        public const string ServiceFee = "M";
    }

    public static class IoFlag
    {
        public const string Incoming = "I";
        public const string Outgoing = "O";
    }

    public static class CardDci
    {
        public const string Debit = "D";
        public const string Prepaid = "P";
        public const string Credit = "C";
    }

    public static class ControlStat
    {
        public const string Normal = "N";
        public const string Problem = "P";
        public const string AccountingClosing = "D";
        public const string ProblemToNormal = "X";
        public const string DisputeEnd = "Y";
    }

}
