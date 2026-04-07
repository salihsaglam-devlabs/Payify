using LinkPara.Card.Domain.Enums;

namespace LinkPara.Card.Domain.Constants;

public static class FileTypeLookupCodes
{
    public static class CardTransaction
    {
        public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> FieldCodes
            = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["TxnInstallType"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CardLookupCodes.TxnInstallType.Normal] = "Normal",
                    [CardLookupCodes.TxnInstallType.Installment] = "Installment",
                    [CardLookupCodes.TxnInstallType.InstallmentWithInterest] = "InstallmentWithInterest",
                    [CardLookupCodes.TxnInstallType.InstallmentWithInstruction] = "InstallmentWithInstruction"
                },
                ["FinancialType"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CardLookupCodes.FinancialType.Capital] = "Capital",
                    [CardLookupCodes.FinancialType.Fee] = "Fee",
                    [CardLookupCodes.FinancialType.Interest] = "Interest",
                    [CardLookupCodes.FinancialType.Tax1] = "Tax1",
                    [CardLookupCodes.FinancialType.Tax2] = "Tax2",
                    [CardLookupCodes.FinancialType.Payment] = "Payment",
                    [CardLookupCodes.FinancialType.Point] = "Point"
                },
                ["TxnEffect"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CardLookupCodes.TxnEffect.Debit] = "DebitEffect",
                    [CardLookupCodes.TxnEffect.Credit] = "CreditEffect",
                    [CardLookupCodes.TxnEffect.CreditPoint] = "CreditEffectPoint",
                    [CardLookupCodes.TxnEffect.DebitPoint] = "DebitEffectPoint",
                    [CardLookupCodes.TxnEffect.Refund] = "RefundEffect"
                },
                ["TxnSource"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CardLookupCodes.TxnSource.Onus] = "Onus",
                    [CardLookupCodes.TxnSource.Domestic] = "Domestic",
                    [CardLookupCodes.TxnSource.Visa] = "Visa",
                    [CardLookupCodes.TxnSource.Mastercard] = "Mastercard"
                },
                ["TxnRegion"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CardLookupCodes.TxnRegion.Onus] = "Onus",
                    [CardLookupCodes.TxnRegion.Domestic] = "Domestic",
                    [CardLookupCodes.TxnRegion.International] = "International",
                    [CardLookupCodes.TxnRegion.IntraRegional] = "IntraRegional"
                },
                ["TerminalType"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CardLookupCodes.TerminalType.Pos] = "POS",
                    [CardLookupCodes.TerminalType.Atm] = "ATM",
                    [CardLookupCodes.TerminalType.Epos] = "EPOS",
                    [CardLookupCodes.TerminalType.InternetBanking] = "InternetBanking",
                    [CardLookupCodes.TerminalType.Ivr] = "IVR",
                    [CardLookupCodes.TerminalType.VirtualPos] = "VirtualPOS",
                    [CardLookupCodes.TerminalType.Crt] = "CRT",
                    [CardLookupCodes.TerminalType.BranchScreen] = "Branch",
                    [CardLookupCodes.TerminalType.UnattendedPos] = "UnattendedPOS",
                    [CardLookupCodes.TerminalType.Validator] = "Validator",
                    [CardLookupCodes.TerminalType.Kiosk] = "Kiosk"
                },
                ["IsTxnSettle"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CardLookupCodes.Flag.Yes] = "Yes",
                    [CardLookupCodes.Flag.No] = "No"
                },
                ["IsSuccessfulTxn"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CardLookupCodes.Flag.Yes] = "Yes",
                    [CardLookupCodes.Flag.No] = "No"
                },
                ["TxnStat"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [CardLookupCodes.TxnStat.Normal] = "Normal",
                    [CardLookupCodes.TxnStat.Reversal] = "Reversal",
                    [CardLookupCodes.TxnStat.Void] = "Void",
                    [CardLookupCodes.TxnStat.Expire] = "Expired"
                },
                ["TxnOrigin"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["0"] = "Authorization",
                    ["1"] = "UserGenerated",
                    ["2"] = "SystemGenerated",
                    ["3"] = "ChannelGenerated"
                }
            };

        public static bool TryGetFieldCodes(string fieldName, out IReadOnlyDictionary<string, string> codes)
        {
            return FieldCodes.TryGetValue(fieldName, out codes);
        }
    }

    public static class Clearing
    {
        private static readonly IReadOnlyDictionary<string, string> DisputeCode = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["H"] = "FeeRepClosing",
            ["M"] = "OutCbCardholderCredit",
            ["U"] = "MerchantCbCardholderCredit",
            ["B"] = "Loss",
            ["E"] = "AccountingCure",
            ["C"] = "RepCardholderCure",
            ["G"] = "OutCbTemporaryAccount",
            ["F"] = "MerchantCbTemporaryAccount",
            ["Z"] = "ExpiryDifferenceAccountingCure",
            ["-"] = "None",
            ["X"] = "FraudTemporaryAccount",
            ["Y"] = "NegativeExchangeRateDifferences",
            ["R"] = "PlusExchangeRateDifferences",
            ["A"] = "OpenMerchantAccounting",
            ["S"] = "Insurance",
            ["T"] = "RejectedExpenseSecurity",
            ["J"] = "PurchasingInsurance",
            ["I"] = "AcceptedLoss",
            ["D"] = "FraudCommission",
            ["V"] = "AcceptedLossSecurity",
            ["P"] = "FraudTemporarily",
            ["N"] = "OutCbWithFeeCharge",
            ["L"] = "OutCbFeeWithSuspenseAccount"
        };

        public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> CommonFieldCodes
            = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["TxnType"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [ClearingLookupCodes.TxnType.Issuer] = "Issuer",
                    [ClearingLookupCodes.TxnType.Acquirer] = "Acquirer",
                    [ClearingLookupCodes.TxnType.Document] = "Document",
                    [ClearingLookupCodes.TxnType.Fee] = "Fee",
                    [ClearingLookupCodes.TxnType.Fraud] = "Fraud",
                    [ClearingLookupCodes.TxnType.ServiceFee] = "ServiceFee"
                },
                ["IoFlag"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [ClearingLookupCodes.IoFlag.Incoming] = "Incoming",
                    [ClearingLookupCodes.IoFlag.Outgoing] = "Outgoing"
                },
                ["CardDCI"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [ClearingLookupCodes.CardDci.Debit] = "Debit",
                    [ClearingLookupCodes.CardDci.Prepaid] = "Prepaid",
                    [ClearingLookupCodes.CardDci.Credit] = "Credit"
                },
                ["ControlStat"] = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [ClearingLookupCodes.ControlStat.Normal] = "Normal",
                    [ClearingLookupCodes.ControlStat.Problem] = "Problem",
                    [ClearingLookupCodes.ControlStat.AccountingClosing] = "AccountingClosing",
                    [ClearingLookupCodes.ControlStat.ProblemToNormal] = "ProblemToNormal",
                    [ClearingLookupCodes.ControlStat.DisputeEnd] = "DisputeEnd"
                }
            };

        public static class Bkm
        {
            public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> FieldCodes
                = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["DisputeCode"] = DisputeCode
                };
        }

        public static class Msc
        {
            public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> FieldCodes
                = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["DisputeCode"] = DisputeCode
                };
        }

        public static class Visa
        {
            public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> FieldCodes
                = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
                {
                    ["DisputeCode"] = DisputeCode
                };
        }

        public static bool TryGetFieldCodes(
            ClearingProvider provider,
            string fieldName,
            out IReadOnlyDictionary<string, string> codes)
        {
            codes = null;
            var providerCodes = provider switch
            {
                ClearingProvider.Bkm => Bkm.FieldCodes,
                ClearingProvider.Mastercard => Msc.FieldCodes,
                ClearingProvider.Visa => Visa.FieldCodes,
                _ => null
            };

            if (providerCodes is not null && providerCodes.TryGetValue(fieldName, out codes))
            {
                return true;
            }

            return CommonFieldCodes.TryGetValue(fieldName, out codes);
        }
    }
}
