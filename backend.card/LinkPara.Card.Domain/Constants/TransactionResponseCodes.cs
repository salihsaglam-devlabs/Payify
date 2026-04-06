
namespace LinkPara.Card.Domain.Constants
{
    public static class TransactionResponseCodes
    {
        public const string Approved = "00";
        public const string NotApproved = "05";
        public const string InvalidOperation = "12";
        public const string InvalidAmount = "13";
        public const string InvalidAccountNo = "14";
        public const string OriginalNotFound = "29";
        public const string InsufficientLimit = "51";
        public const string DisallowedCardTransaction = "57";
        public const string ExceededCashLimit = "61";
        public const string ExceedPinTryLimit = "65";
        public const string McrDuplicatedTransaction = "94";
        public const string SystemError96 = "96";
    }
}
