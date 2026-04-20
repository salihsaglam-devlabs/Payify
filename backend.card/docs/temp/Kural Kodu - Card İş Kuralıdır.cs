#nullable enable

using System;
using System.Collections.Generic;

namespace Payify.CardTransactionReference;

public static class CardTransactionDrawioFlowHelper
{
    public static FlowResult Execute(FlowInput input)
    {
        var actions = new List<FlowAction>();

        // C1*
        if (!input.IsFileLengthControlSuccessful)
        {
            actions.Add(new FlowAction("C1*", "Alarm oluşturulur"));
            return new FlowResult("C1*(HAYIR)", "Alarm", actions);
        }

        // C2*
        if (!input.AreRecordsUnique)
        {
            actions.Add(new FlowAction("C2*", "Alarm oluşturulur"));
            return new FlowResult("C2*(HAYIR)", "Alarm", actions);
        }

        // C3*
        if (input.IsCancelOrReversal)
        {
            // İşlem iptal edilmiş mi?
            if (input.IsTransactionCancelled)
            {
                return new FlowResult("C3*(EVET)>İşlem iptal edilmiş mi?(EVET)>END", "NoAction", actions);
            }

            actions.Add(new FlowAction("C4*", "İşlem bakiye düzeltme listesine alınır"));
            AddD7(actions);
            return new FlowResult("C3*(EVET)>İşlem iptal edilmiş mi?(HAYIR)>C4*>D7", "D7", actions);
        }

        // C5*
        switch (input.FileTransactionStatus)
        {
            case FileTransactionStatus.Basarisiz:
                return ExecuteFailedBranch(input, actions);

            case FileTransactionStatus.Expire:
                return ExecuteExpireBranch(input, actions);

            case FileTransactionStatus.Basarili:
                return ExecuteSuccessfulBranch(input, actions);

            default:
                return new FlowResult("C5*", "NoAction", actions);
        }
    }

    private static FlowResult ExecuteFailedBranch(FlowInput input, List<FlowAction> actions)
    {
        // C19*
        switch (input.PayifyTransactionStatus)
        {
            case PayifyTransactionStatus.Basarisiz:
                return new FlowResult(
                    "C5*(BAŞARISIZ)>C19*(BAŞARISIZ)>END",
                    "NoAction",
                    actions);

            case PayifyTransactionStatus.Yok:
                return new FlowResult(
                    "C5*(BAŞARISIZ)>C19*(YOK)>END",
                    "NoAction",
                    actions);

            case PayifyTransactionStatus.Basarili:
                actions.Add(new FlowAction("C6*", "İşlem bakiye düzeltme listesine alınır"));
                AddD1(actions);
                return new FlowResult(
                    "C5*(BAŞARISIZ)>C19*(BAŞARILI)>C6*>D1",
                    "D1",
                    actions);

            default:
                return new FlowResult("C5*(BAŞARISIZ)", "NoAction", actions);
        }
    }

    private static FlowResult ExecuteExpireBranch(FlowInput input, List<FlowAction> actions)
    {
        // C19*
        switch (input.PayifyTransactionStatus)
        {
            case PayifyTransactionStatus.Basarisiz:
                actions.Add(new FlowAction("C7*", "İşlem statüsü EXPIRE'a çekilir"));
                return new FlowResult(
                    "C5*(EXPIRE)>C19*(BAŞARISIZ)>C7*>END",
                    "C7*",
                    actions);

            case PayifyTransactionStatus.Yok:
                actions.Add(new FlowAction("C8*", "İşlemi oluştur & işlem statüsü EXPIRE'a çekilir"));
                return new FlowResult(
                    "C5*(EXPIRE)>C19*(YOK)>C8*>END",
                    "C8*",
                    actions);

            case PayifyTransactionStatus.Basarili:
                // C9*
                if (input.HasAccControlStatP)
                {
                    actions.Add(new FlowAction("C10*", "Alarm Üret/Manuel Kontrol Gerektiren İşlem"));
                    return new FlowResult(
                        "C5*(EXPIRE)>C19*(BAŞARILI)>C9*(EVET)>C10*>END",
                        "C10*",
                        actions);
                }

                actions.Add(new FlowAction("C11*", "İşlem bakiye düzeltme listesine alınır"));
                AddD2(actions);
                return new FlowResult(
                    "C5*(EXPIRE)>C19*(BAŞARILI)>C9*(HAYIR)>C11*>D2",
                    "D2",
                    actions);

            default:
                return new FlowResult("C5*(EXPIRE)", "NoAction", actions);
        }
    }

    private static FlowResult ExecuteSuccessfulBranch(FlowInput input, List<FlowAction> actions)
    {
        // TxnSettle=Y mi?
        if (!input.IsTxnSettleY)
        {
            return new FlowResult(
                "C5*(BAŞARILI)>TxnSettle(HAYIR)>END",
                "NoAction",
                actions);
        }

        // C19*
        switch (input.PayifyTransactionStatus)
        {
            case PayifyTransactionStatus.Basarisiz:
                actions.Add(new FlowAction("C12*", "İşlem bakiye düzeltme listesine alınır"));
                AddD3(actions);
                return new FlowResult(
                    "C5*(BAŞARILI)>TxnSettle(EVET)>C19*(BAŞARISIZ)>C12*>D3",
                    "D3",
                    actions);

            case PayifyTransactionStatus.Yok:
                actions.Add(new FlowAction("C13*", "İşlem yaratılır"));

                // C14* - İADE işlemi mi?
                if (!input.IsRefundTransaction)
                {
                    actions.Add(new FlowAction("C14*", "İşlem bakiye düzeltme listesine alınır"));
                    AddD4(actions);
                    return new FlowResult(
                        "C5*(BAŞARILI)>TxnSettle(EVET)>C19*(YOK)>C13*>İADE işlemi mi?(HAYIR)>C14*>D4",
                        "D4",
                        actions);
                }

                // C15* - İADE eşlenikli mi?
                if (input.IsMatchedRefund)
                {
                    actions.Add(new FlowAction("C17*", "İşlem bakiye düzeltme listesine alınır"));
                    AddD5(actions);
                    return new FlowResult(
                        "C5*(BAŞARILI)>TxnSettle(EVET)>C19*(YOK)>C13*>İADE işlemi mi?(EVET)>C15*(EVET)>C17*>D5",
                        "D5",
                        actions);
                }

                actions.Add(new FlowAction("C16*", "Manuel kontrol bekleyen işlem listesine ilet"));
                AddD6(actions);
                return new FlowResult(
                    "C5*(BAŞARILI)>TxnSettle(EVET)>C19*(YOK)>C13*>İADE işlemi mi?(EVET)>C15*(HAYIR)>C16*>D6",
                    "D6",
                    actions);

            case PayifyTransactionStatus.Basarili:
                // İADE işlemi mi?
                if (input.IsRefundTransaction)
                {
                    // C15*
                    if (input.IsMatchedRefund)
                    {
                        actions.Add(new FlowAction("C17*", "İşlem bakiye düzeltme listesine alınır"));
                        AddD5(actions);
                        return new FlowResult(
                            "C5*(BAŞARILI)>TxnSettle(EVET)>C19*(BAŞARILI)>İADE işlemi mi?(EVET)>C15*(EVET)>C17*>D5",
                            "D5",
                            actions);
                    }

                    actions.Add(new FlowAction("C16*", "Manuel kontrol bekleyen işlem listesine ilet"));
                    AddD6(actions);
                    return new FlowResult(
                        "C5*(BAŞARILI)>TxnSettle(EVET)>C19*(BAŞARILI)>İADE işlemi mi?(EVET)>C15*(HAYIR)>C16*>D6",
                        "D6",
                        actions);
                }

                // Amount compare
                if (input.IsTransactionAmountEqualToBillingAmount)
                {
                    return new FlowResult(
                        "C5*(BAŞARILI)>TxnSettle(EVET)>C19*(BAŞARILI)>İADE işlemi mi?(HAYIR)>Amount=BillingAmount>END",
                        "NoAction",
                        actions);
                }

                if (input.IsTransactionAmountLessThanBillingAmount)
                {
                    AddD8(actions);
                    return new FlowResult(
                        "C5*(BAŞARILI)>TxnSettle(EVET)>C19*(BAŞARILI)>İADE işlemi mi?(HAYIR)>Amount!=BillingAmount>Amount<BillingAmount>D8",
                        "D8",
                        actions);
                }

                return new FlowResult(
                    "C5*(BAŞARILI)>TxnSettle(EVET)>C19*(BAŞARILI)>İADE işlemi mi?(HAYIR)>Amount!=BillingAmount>Amount>=BillingAmount>END",
                    "NoAction",
                    actions);

            default:
                return new FlowResult("C5*(BAŞARILI)", "NoAction", actions);
        }
    }

    private static void AddD1(List<FlowAction> actions)
    {
        actions.Add(new FlowAction("D1", "İşlem Bakiye Düzeltme"));
        actions.Add(new FlowAction("C1*", "Response Code düzelt; BAŞARILI işlemi BAŞARISIZ'a çevir"));
        actions.Add(new FlowAction("D1", "Bakiye efektine göre tersine çevir"));
    }

    private static void AddD2(List<FlowAction> actions)
    {
        actions.Add(new FlowAction("D2", "İşlem Bakiye Düzeltme"));
        actions.Add(new FlowAction("C2*", "İşlem EXPIRE'a çekilir"));
        actions.Add(new FlowAction("D2", "Bakiye efektine göre tersine çevir"));
    }

    private static void AddD3(List<FlowAction> actions)
    {
        actions.Add(new FlowAction("D3", "İşlem Bakiye Düzeltme"));
        actions.Add(new FlowAction("C3*", "Response Code düzelt; BAŞARISIZ işlemi BAŞARILI'ya çevir"));
        actions.Add(new FlowAction("D3", "Bakiye efektine göre tersine çevir"));
    }

    private static void AddD4(List<FlowAction> actions)
    {
        actions.Add(new FlowAction("D4", "İşlem Bakiye Düzeltme"));
        actions.Add(new FlowAction("C4*", "İşlem oluşturulur"));
        actions.Add(new FlowAction("D4", "İşlemin orijinaline göre iade işlemi yapılır"));
    }

    private static void AddD5(List<FlowAction> actions)
    {
        actions.Add(new FlowAction("D5", "İşlem Bakiye Düzeltme"));
        actions.Add(new FlowAction("C5*", "(Eşlenikli İade) İşlemin orijinaline göre iade işlemi yapılır"));
    }

    private static void AddD6(List<FlowAction> actions)
    {
        actions.Add(new FlowAction("D6", "İşlem Bakiye Düzeltme"));
        actions.Add(new FlowAction("C6*", "Manuel kontrol bekleyen işlem listesine ilet"));
    }

    private static void AddD7(List<FlowAction> actions)
    {
        actions.Add(new FlowAction("D7", "İşlem Bakiye Düzeltme"));
        actions.Add(new FlowAction("D7", "İptal ya da Reversal; Orijinal işlemin IsCancelled alanı=1 yapılır"));
        actions.Add(new FlowAction("D7", "İşlemin orijinali tersine çevrilir"));
    }

    private static void AddD8(List<FlowAction> actions)
    {
        actions.Add(new FlowAction("D8", "İşlem Bakiye Düzeltme"));
        actions.Add(new FlowAction("D8", "Fark tutarı kadar Borç/Alacak tablosuna yazılır"));
    }
}

public sealed record FlowInput(
    bool IsFileLengthControlSuccessful,
    bool AreRecordsUnique,
    bool IsCancelOrReversal,
    bool IsTransactionCancelled,
    FileTransactionStatus FileTransactionStatus,
    PayifyTransactionStatus PayifyTransactionStatus,
    bool HasAccControlStatP,
    bool IsTxnSettleY,
    bool IsRefundTransaction,
    bool IsMatchedRefund,
    bool IsTransactionAmountEqualToBillingAmount,
    bool IsTransactionAmountLessThanBillingAmount);

public sealed record FlowResult(
    string Path,
    string ResultCode,
    IReadOnlyCollection<FlowAction> Actions);

public sealed record FlowAction(
    string Code,
    string Description);

public enum FileTransactionStatus
{
    Basarisiz = 1,
    Expire = 2,
    Basarili = 3
}

public enum PayifyTransactionStatus
{
    Basarisiz = 1,
    Yok = 2,
    Basarili = 3
}