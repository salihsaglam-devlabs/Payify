namespace LinkPara.Card.Domain.Constants
{
    public static class TxnTypes
    {
        public const string SALE = "SALE";
        public const string INSURANCE = "INSURANCE";
        public const string INQUIRYCASHINCOM = "INQUIRYCASHINCOM";
        public const string ATMCASHIN = "ATMCASHIN";
        public const string INQUIRYWITHDRAWALCOM = "INQUIRYWITHDRAWALCOM";
        public const string WITHDRAWAL = "WITHDRAWAL";
        public const string INQUIRYBALANCECOM = "INQUIRYBALANCECOM";
        public const string INQUIRYBALANCE = "INQUIRYBALANCE";
        public const string REFUND = "REFUND";
        public const string REFERENCEDREFUND = "REFERENCEDREFUND";
        public const string INQUIRY = "INQUIRY";
        public const string BILLPAYMENT = "BILLPAYMENT";
        public const string INQUIRYKKPT = "INQUIRYKKPT";
        public const string KKPT = "KKPT";

        public static List<String> ALL = new List<String>(){ "SALE", "INSURANCE", "INQUIRYCASHINCOM", "ATMCASHIN", "INQUIRYWITHDRAWALCOM",
                       "WITHDRAWAL", "INQUIRYBALANCECOM", "INQUIRYBALANCE", "REFUND", "REFERENCEDREFUND", "INQUIRY",
                        "BILLPAYMENT", "INQUIRYKKPT", "KKPT" };
    }
}
