
namespace LinkPara.Card.Domain.Constants
{
    public static class EmoneyResponseCodes
    {
        public const string Approved = "EMN000";
        public const string ApprovedMsg = "Successful";

        public const string NotApproved = "EMN003";
        public const string NotApprovedMsg = "Invalid wallet !";

        public const string NotApproved2 = "EMN013";
        public const string NotApproved2Msg = "The entity's record status already passivated!";

        public const string NotApproved3 = "EMN014";
        public const string NotApproved3Msg = "EMN014";

        public const string NotApproved4 = "EMN017";
        public const string NotApproved4Msg = "Invalid Profile Item !";

        public const string NotApproved5 = "EMN035";
        public const string NotApproved5Msg = "Status Not Acceptable";

        public const string InvalidOperation = "EMN006";
        public const string InvalidOperationMsg = "Invalid transaction !";

        public const string InvalidOperation2 = "EMN020";
        public const string InvalidOperation2Msg = "Duplicate Wallet!";

        public const string InvalidAmount = "EMN021";
        public const string InvalidAmountMsg = "Invalid Amount !";

        public const string InvalidAmount2 = "EMN022";
        public const string InvalidAmount2Msg = "Amount can not be Empty !";

        public const string InvalidAmount3 = "EMN023";
        public const string InvalidAmount3Msg = "Request amount is not greater than provision amount !";

        public const string InvalidAccountNo = "EMN044";
        public const string InvalidAccountNoMsg = "User not found";

        public const string InsufficientLimit = "EMN002";
        public const string InsufficientLimitMsg = "Insufficient balance !";

        public const string InsufficientLimit2 = "EMN040";
        public const string InsufficientLimit2Msg = "Same acccount insufficient balance !";

        public const string DisallowedCardTransaction = "EMN004";
        public const string DisallowedCardTransactionMsg = "Currency code mismatch !";

        public const string DisallowedCardTransaction2 = "EMN010";
        public const string DisallowedCardTransaction2Msg = "Invalid tier level!";

        public const string DisallowedCardTransaction3 = "EMN011";
        public const string DisallowedCardTransaction3Msg = "EMN011";

        public const string DisallowedCardTransaction4 = "EMN015";
        public const string DisallowedCardTransaction4Msg = "Wallet blocked !";

        public const string DisallowedCardTransaction5 = "EMN019";
        public const string DisallowedCardTransaction5Msg = "Invalid Customer Level !";

        public const string DisallowedCardTransaction6 = "EMN026";
        public const string DisallowedCardTransaction6Msg = "Commercial pricing record is invalid!";

        public const string DisallowedCardTransaction7 = "EMN028";
        public const string DisallowedCardTransaction7Msg = "Account tier is not permitted for this operation !";

        public const string ExceededCashLimit = "EMN012";
        public const string ExceededCashLimitMsg = "Limit exceeded !";

        public const string ExceedPinTryLimit = "EMN041";
        public const string ExceedPinTryLimitMsg = "Insufficient account limit !";

        public const string McrDuplicatedTransaction = "EMN037";
        public const string McrDuplicatedTransactionMsg = "Already Processed !";

        public const string McrDuplicatedTransaction2 = "EMN039";
        public const string McrDuplicatedTransaction2Msg = "Duplicate Bulk Transfer Detail Found !";

        public const string SystemError96 = "EMN024";
        public const string SystemError96Msg = "Error occurred during withdrawal request !";

        public const string SystemError96_2 = "EMN031";
        public const string SystemError96_2Msg = "EMN031";

        public const string SystemError96_3 = "EMN032";
        public const string SystemError96_3Msg = "Phone Number is already in use !";

        public const string SystemError96_4 = "EMN036";
        public const string SystemError96_4Msg = "Process Time Out";
        
        public const string OriginalNotFoundMsg = "Original transaction not found!";
    }
}
