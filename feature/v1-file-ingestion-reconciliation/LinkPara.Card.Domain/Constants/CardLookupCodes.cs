namespace LinkPara.Card.Domain.Constants;

public static class CardLookupCodes
{
    public static class TxnInstallType
    {
        public const string Normal = "N";
        public const string Installment = "Y";
        public const string InstallmentWithInterest = "I";
        public const string InstallmentWithInstruction = "T";
    }

    public static class FinancialType
    {
        public const string Capital = "C";
        public const string Fee = "F";
        public const string Interest = "I";
        public const string Tax1 = "B";
        public const string Tax2 = "K";
        public const string Payment = "M";
        public const string Point = "P";
    }

    public static class TxnEffect
    {
        public const string Debit = "D";
        public const string Credit = "C";
        public const string CreditPoint = "P";
        public const string DebitPoint = "M";
        public const string Refund = "R";
    }

    public static class TxnSource
    {
        public const string Onus = "O";
        public const string Domestic = "N";
        public const string Visa = "V";
        public const string Mastercard = "M";
    }

    public static class TxnRegion
    {
        public const string Onus = "O";
        public const string Domestic = "D";
        public const string International = "I";
        public const string IntraRegional = "R";
    }

    public static class TerminalType
    {
        public const string Pos = "PO";
        public const string Atm = "AT";
        public const string Epos = "EP";
        public const string InternetBanking = "IN";
        public const string Ivr = "IV";
        public const string VirtualPos = "VP";
        public const string Crt = "CT";
        public const string BranchScreen = "BR";
        public const string UnattendedPos = "UA";
        public const string Validator = "VL";
        public const string Kiosk = "KI";
    }

    public static class TxnStat
    {
        public const string Normal = "N";
        public const string Reversal = "R";
        public const string Void = "V";
        public const string Expire = "E";
    }

    public static class Flag
    {
        public const string Yes = "Y";
        public const string No = "N";
    }
}
