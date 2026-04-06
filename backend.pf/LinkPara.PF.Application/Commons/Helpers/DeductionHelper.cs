using LinkPara.PF.Application.Commons.Models.MerchantDeductions;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Helpers;

public static class DeductionHelper
{
    public static DeductionResult Calculate(
        List<PostingBalance> postingBalances, 
        List<MerchantDeduction> merchantDeductions,
        List<PostingTransaction> deductionPostingTransactions,
        Guid applicationUserId)
    {
        var result  = new DeductionResult
        {
            DeductionTransactions = [],
            PostingAdditionalTransactions = [],
            MerchantDeductions = merchantDeductions,
            PostingBalances = postingBalances.OrderByDescending(s => s.TotalPayingAmount).ThenBy(w => w.BlockageStatus).ToList()
        };
        
        var deductionsLookup = merchantDeductions
            .Where(x => x.DeductionStatus == DeductionStatus.Processing)
            .GroupBy(x => (x.MerchantId, x.Currency))
            .ToDictionary(g => g.Key, g => g.ToList());
        
        foreach (var balance in result.PostingBalances)
        {
            var totalPayingAmount = balance.TotalPayingAmount;
            var totalChargebackAmount = balance.TotalChargebackAmount;
            var totalSuspiciousAmount = balance.TotalSuspiciousAmount;
            var totalDueAmount = balance.TotalDueAmount;
            var totalExcessReturnAmount = balance.TotalExcessReturnAmount;
            var totalAmount = balance.TotalAmount;
            var totalBankCommissionAmount = balance.TotalBankCommissionAmount;
            var totalAmountWithoutBankCommission = balance.TotalAmountWithoutBankCommission;
            var totalPfCommissionAmount = balance.TotalPfCommissionAmount;
            var totalPfNetCommissionAmount = balance.TotalPfNetCommissionAmount;
            var totalAmountWithoutCommissions = balance.TotalAmountWithoutCommissions;
            var totalTransactionCount = balance.TransactionCount;
            var totalDueTransferAmount = balance.TotalDueTransferAmount;
            var totalChargebackCommissionAmount = balance.TotalChargebackCommissionAmount;
            var totalChargebackTransferAmount = balance.TotalChargebackTransferAmount;
            var totalSuspiciousCommissionAmount = balance.TotalSuspiciousCommissionAmount;
            var totalSuspiciousTransferAmount = balance.TotalSuspiciousTransferAmount;
            var totalExcessReturnTransferAmount = balance.TotalExcessReturnTransferAmount;
            var totalExcessReturnOnCommissionAmount = balance.TotalExcessReturnOnCommissionAmount;

            if (!deductionsLookup.TryGetValue((balance.MerchantId, balance.Currency),
                    out var merchantSpecifiedDeductions))
            {
                balance.BatchStatus = BatchStatus.DeductionCalculated;
                continue;
            }

            foreach (var merchantDeduction in merchantSpecifiedDeductions)
            {
                var deductionAmount = 0m;
                if (merchantDeduction.RemainingDeductionAmount <= totalPayingAmount)
                {
                    totalPayingAmount -= merchantDeduction.RemainingDeductionAmount;
                    totalChargebackAmount += merchantDeduction.DeductionType is DeductionType.Chargeback
                        ? merchantDeduction.RemainingDeductionAmount
                        : 0;
                    totalSuspiciousAmount += merchantDeduction.DeductionType is DeductionType.Suspicious
                        ? merchantDeduction.RemainingDeductionAmount
                        : 0;
                    totalDueAmount += merchantDeduction.DeductionType == DeductionType.Due
                        ? merchantDeduction.RemainingDeductionAmount
                        : 0;
                    totalExcessReturnAmount += merchantDeduction.DeductionType == DeductionType.ExcessReturn
                        ? merchantDeduction.RemainingDeductionAmount
                        : 0;

                    totalDueTransferAmount += merchantDeduction.DeductionType == DeductionType.DueTransfer
                        ? merchantDeduction.RemainingDeductionAmount
                        : 0;
                    totalChargebackCommissionAmount +=
                        merchantDeduction.DeductionType == DeductionType.ChargebackCommission
                            ? merchantDeduction.RemainingDeductionAmount
                            : 0;
                    totalChargebackTransferAmount += merchantDeduction.DeductionType == DeductionType.ChargebackTransfer
                        ? merchantDeduction.RemainingDeductionAmount
                        : 0;
                    totalSuspiciousCommissionAmount +=
                        merchantDeduction.DeductionType == DeductionType.SuspiciousCommission
                            ? merchantDeduction.RemainingDeductionAmount
                            : 0;
                    totalSuspiciousTransferAmount += merchantDeduction.DeductionType == DeductionType.SuspiciousTransfer
                        ? merchantDeduction.RemainingDeductionAmount
                        : 0;
                    totalExcessReturnTransferAmount +=
                        merchantDeduction.DeductionType == DeductionType.ExcessReturnTransfer
                            ? merchantDeduction.RemainingDeductionAmount
                            : 0;
                    totalExcessReturnOnCommissionAmount +=
                        merchantDeduction.DeductionType == DeductionType.ExcessReturnOnCommission
                            ? merchantDeduction.RemainingDeductionAmount
                            : 0;

                    deductionAmount = merchantDeduction.RemainingDeductionAmount;
                    merchantDeduction.RemainingDeductionAmount = 0;
                    merchantDeduction.DeductionStatus = DeductionStatus.Completed;
                    merchantDeduction.ProcessingId = null;
                }
                else
                {
                    merchantDeduction.RemainingDeductionAmount -= totalPayingAmount;
                    totalChargebackAmount += merchantDeduction.DeductionType is DeductionType.Chargeback
                        ? totalPayingAmount
                        : 0;
                    totalSuspiciousAmount += merchantDeduction.DeductionType is DeductionType.Suspicious
                        ? totalPayingAmount
                        : 0;
                    totalDueAmount += merchantDeduction.DeductionType == DeductionType.Due ? totalPayingAmount : 0;
                    totalExcessReturnAmount += merchantDeduction.DeductionType == DeductionType.ExcessReturn
                        ? totalPayingAmount
                        : 0;

                    totalDueTransferAmount += merchantDeduction.DeductionType == DeductionType.DueTransfer
                        ? totalPayingAmount
                        : 0;
                    totalChargebackCommissionAmount +=
                        merchantDeduction.DeductionType == DeductionType.ChargebackCommission ? totalPayingAmount : 0;
                    totalChargebackTransferAmount += merchantDeduction.DeductionType == DeductionType.ChargebackTransfer
                        ? totalPayingAmount
                        : 0;
                    totalSuspiciousCommissionAmount +=
                        merchantDeduction.DeductionType == DeductionType.SuspiciousCommission ? totalPayingAmount : 0;
                    totalSuspiciousTransferAmount += merchantDeduction.DeductionType == DeductionType.SuspiciousTransfer
                        ? totalPayingAmount
                        : 0;
                    totalExcessReturnTransferAmount +=
                        merchantDeduction.DeductionType == DeductionType.ExcessReturnTransfer ? totalPayingAmount : 0;
                    totalExcessReturnOnCommissionAmount +=
                        merchantDeduction.DeductionType == DeductionType.ExcessReturnOnCommission
                            ? totalPayingAmount
                            : 0;

                    deductionAmount = totalPayingAmount;
                    totalPayingAmount = 0;
                }
                
                if (deductionAmount != 0)
                {
                    var deductionTransaction = new DeductionTransaction
                    {
                        MerchantId = merchantDeduction.MerchantId,
                        PostingBalanceId = balance.Id,
                        MerchantDeductionId = merchantDeduction.Id,
                        DeductionType = merchantDeduction.DeductionType,
                        Amount = deductionAmount,
                        Currency = merchantDeduction.Currency,
                        CreatedBy = applicationUserId.ToString()
                    };

                    result.DeductionTransactions.Add(deductionTransaction);

                    var postingAdditionalTransaction = new PostingAdditionalTransaction
                    {
                        MerchantId = merchantDeduction.MerchantId,
                        PostingDate = balance.PostingDate,
                        PaymentDate = balance.PaymentDate,
                        Currency = merchantDeduction.Currency,
                        PointAmount = 0,
                        BatchStatus = BatchStatus.Completed,
                        BlockageStatus = BlockageStatus.None,
                        MerchantDeductionId = merchantDeduction.Id,
                        PostingBankBalanceId = Guid.Empty,
                        PostingBalanceId = balance.Id,
                        BTransStatus = PostingBTransStatus.Completed,
                        CreatedBy = merchantDeduction.CreatedBy,
                        RelatedPostingBalanceId = Guid.Empty,
                        SubMerchantId = Guid.Empty,
                        SubMerchantName = string.Empty,
                        SubMerchantNumber = string.Empty,
                        EasySubMerchantName = string.Empty,
                        EasySubMerchantNumber = string.Empty,
                        PfTransactionSource = PfTransactionSource.VirtualPos,
                        MerchantPhysicalPosId = Guid.Empty,
                        InstallmentSequence = 0,
                        IsPerInstallment = false,
                        MerchantInstallmentTransactionId = Guid.Empty
                    };

                    var originalPostingTransaction = deductionPostingTransactions.FirstOrDefault(s =>
                        s.MerchantTransactionId == merchantDeduction.MerchantTransactionId);

                    var suspiciousDeductionGroup = new List<DeductionType>
                    {
                        DeductionType.Suspicious,
                        DeductionType.SuspiciousCommission,
                        DeductionType.SuspiciousTransfer,
                    };

                    var chargebackDeductionGroup = new List<DeductionType>
                    {
                        DeductionType.Chargeback,
                        DeductionType.ChargebackCommission,
                        DeductionType.ChargebackTransfer,
                    };

                    if (suspiciousDeductionGroup.Contains(merchantDeduction.DeductionType) ||
                        chargebackDeductionGroup.Contains(merchantDeduction.DeductionType))
                    {
                        postingAdditionalTransaction.TransactionType =
                            suspiciousDeductionGroup.Contains(merchantDeduction.DeductionType)
                                ? TransactionType.Suspicious
                                : TransactionType.Chargeback;
                        postingAdditionalTransaction.TransactionDate = originalPostingTransaction.TransactionDate;
                        postingAdditionalTransaction.OldPaymentDate = originalPostingTransaction.PaymentDate;
                        postingAdditionalTransaction.CardNumber = originalPostingTransaction.CardNumber;
                        postingAdditionalTransaction.OrderId = originalPostingTransaction.OrderId;
                        postingAdditionalTransaction.InstallmentCount = originalPostingTransaction.InstallmentCount;
                        postingAdditionalTransaction.PricingProfileNumber =
                            originalPostingTransaction.PricingProfileNumber;
                        postingAdditionalTransaction.MerchantTransactionId =
                            originalPostingTransaction.MerchantTransactionId;
                        postingAdditionalTransaction.PricingProfileItemId =
                            originalPostingTransaction.PricingProfileItemId;
                        postingAdditionalTransaction.VposId = originalPostingTransaction.VposId;
                        postingAdditionalTransaction.AcquireBankCode = originalPostingTransaction.AcquireBankCode;
                        postingAdditionalTransaction.TransactionStartDate =
                            originalPostingTransaction.TransactionStartDate;
                        postingAdditionalTransaction.TransactionEndDate = originalPostingTransaction.TransactionEndDate;
                        postingAdditionalTransaction.ConversationId = originalPostingTransaction.ConversationId;
                    }
                    else
                    {
                        //Due-DueTransfer-ExcessReturn-ExcessReturnTransfer-ExcessReturnOnCommission
                        var transactionType = merchantDeduction.DeductionType switch
                        {
                            DeductionType.ExcessReturn => TransactionType.ExcessReturn,
                            DeductionType.Due => TransactionType.Due,
                            _ => TransactionType.SubMerchantDeduction
                        };

                        postingAdditionalTransaction.TransactionType = transactionType;
                        postingAdditionalTransaction.TransactionDate = DateTime.Now;
                        postingAdditionalTransaction.OldPaymentDate = DateTime.Now;
                        postingAdditionalTransaction.CardNumber = string.Empty;
                        postingAdditionalTransaction.OrderId = string.Empty;
                        postingAdditionalTransaction.InstallmentCount = 0;
                        postingAdditionalTransaction.PricingProfileNumber = string.Empty;
                        postingAdditionalTransaction.MerchantTransactionId = Guid.Empty;
                        postingAdditionalTransaction.PricingProfileItemId = Guid.Empty;
                        postingAdditionalTransaction.VposId = Guid.Empty;
                        postingAdditionalTransaction.AcquireBankCode = 0;
                        postingAdditionalTransaction.TransactionStartDate = DateTime.Now;
                        postingAdditionalTransaction.TransactionEndDate = DateTime.Now;
                        postingAdditionalTransaction.ConversationId = string.Empty;
                    }

                    if (merchantDeduction.TotalDeductionAmount !=
                        merchantDeduction.DeductionAmountWithCommission)
                    {
                        postingAdditionalTransaction.AmountWithoutCommissions = deductionAmount;
                        postingAdditionalTransaction.BankCommissionRate = originalPostingTransaction.BankCommissionRate;
                        postingAdditionalTransaction.PfCommissionRate = originalPostingTransaction.PfCommissionRate;
                        postingAdditionalTransaction.ParentMerchantCommissionRate =
                            originalPostingTransaction.ParentMerchantCommissionRate;
                        postingAdditionalTransaction.PfPerTransactionFee =
                            merchantDeduction.RemainingDeductionAmount == 0
                                ? originalPostingTransaction.PfPerTransactionFee
                                : 0;
                        postingAdditionalTransaction.Amount =
                            (postingAdditionalTransaction.AmountWithoutCommissions +
                             postingAdditionalTransaction.PfPerTransactionFee) / (1 -
                                (postingAdditionalTransaction.PfCommissionRate / 100m) -
                                (postingAdditionalTransaction.ParentMerchantCommissionRate / 100m));
                        postingAdditionalTransaction.PfCommissionAmount =
                            postingAdditionalTransaction.PfPerTransactionFee +
                            postingAdditionalTransaction.PfCommissionRate / 100m * postingAdditionalTransaction.Amount;
                        postingAdditionalTransaction.BankCommissionAmount = (postingAdditionalTransaction.Amount *
                                                                             postingAdditionalTransaction
                                                                                 .BankCommissionRate) / 100m;
                        postingAdditionalTransaction.AmountWithoutBankCommission = postingAdditionalTransaction.Amount -
                            postingAdditionalTransaction.BankCommissionAmount;
                        postingAdditionalTransaction.PfNetCommissionAmount =
                            postingAdditionalTransaction.PfCommissionAmount -
                            postingAdditionalTransaction.BankCommissionAmount;
                        postingAdditionalTransaction.ParentMerchantCommissionAmount =
                            (postingAdditionalTransaction.Amount *
                             postingAdditionalTransaction.ParentMerchantCommissionRate) / 100m;
                        postingAdditionalTransaction.AmountWithoutParentMerchantCommission =
                            postingAdditionalTransaction.Amount -
                            postingAdditionalTransaction.ParentMerchantCommissionAmount;
                    }
                    else
                    {
                        postingAdditionalTransaction.Amount = deductionAmount;
                        postingAdditionalTransaction.BankCommissionRate = 0;
                        postingAdditionalTransaction.BankCommissionAmount = 0;
                        postingAdditionalTransaction.AmountWithoutBankCommission = deductionAmount;
                        postingAdditionalTransaction.PfCommissionRate = 0;
                        postingAdditionalTransaction.PfPerTransactionFee = 0;
                        postingAdditionalTransaction.PfCommissionAmount = 0;
                        postingAdditionalTransaction.PfNetCommissionAmount = 0;
                        postingAdditionalTransaction.AmountWithoutCommissions = deductionAmount;
                        postingAdditionalTransaction.ParentMerchantCommissionAmount = 0;
                        postingAdditionalTransaction.ParentMerchantCommissionRate = 0;
                        postingAdditionalTransaction.AmountWithoutParentMerchantCommission = deductionAmount;
                    }

                    totalAmount -= postingAdditionalTransaction.Amount;
                    totalBankCommissionAmount -= postingAdditionalTransaction.BankCommissionAmount;
                    totalAmountWithoutBankCommission -= postingAdditionalTransaction.AmountWithoutBankCommission;
                    totalPfCommissionAmount -= postingAdditionalTransaction.PfCommissionAmount;
                    totalPfNetCommissionAmount -= postingAdditionalTransaction.PfNetCommissionAmount;
                    totalAmountWithoutCommissions -= postingAdditionalTransaction.AmountWithoutCommissions;
                    totalTransactionCount += 1;

                    result.PostingAdditionalTransactions.Add(postingAdditionalTransaction);
                }

                if (totalPayingAmount <= 0)
                {
                    break;
                }
            }

            balance.BatchStatus = BatchStatus.DeductionCalculated;
            balance.TotalChargebackAmount = totalChargebackAmount;
            balance.TotalSuspiciousAmount = totalSuspiciousAmount;
            balance.TotalDueAmount = totalDueAmount;
            balance.TotalExcessReturnAmount = totalExcessReturnAmount;
            balance.TotalPayingAmount = totalPayingAmount;
            balance.TotalAmount = totalAmount;
            balance.TotalBankCommissionAmount = totalBankCommissionAmount;
            balance.TotalAmountWithoutBankCommission = totalAmountWithoutBankCommission;
            balance.TotalPfCommissionAmount = totalPfCommissionAmount;
            balance.TotalPfNetCommissionAmount = totalPfNetCommissionAmount;
            balance.TotalAmountWithoutCommissions = totalAmountWithoutCommissions;
            balance.TransactionCount = totalTransactionCount;
            balance.TotalDueTransferAmount = totalDueTransferAmount;
            balance.TotalChargebackCommissionAmount = totalChargebackCommissionAmount;
            balance.TotalChargebackTransferAmount = totalChargebackTransferAmount;
            balance.TotalSuspiciousCommissionAmount = totalSuspiciousCommissionAmount;
            balance.TotalSuspiciousTransferAmount = totalSuspiciousTransferAmount;
            balance.TotalExcessReturnTransferAmount = totalExcessReturnTransferAmount;
            balance.TotalExcessReturnOnCommissionAmount = totalExcessReturnOnCommissionAmount;

            if (totalPayingAmount <= 0)
            {
                balance.MoneyTransferStatus = PostingMoneyTransferStatus.PaymentNotRequired;
                balance.BatchStatus = BatchStatus.Completed;
            }
        }
        
        foreach (var s in result.MerchantDeductions.Where(s => s.DeductionStatus != DeductionStatus.Completed))
        {
            s.DeductionStatus = DeductionStatus.Pending;
            s.ProcessingId = null;
        }

        return result;
    }
}