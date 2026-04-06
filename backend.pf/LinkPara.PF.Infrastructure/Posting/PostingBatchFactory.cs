using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Infrastructure.Posting;

public class PostingBatchFactory : IPostingBatchFactory
{
    private readonly IPostingBatch<PostingTransfer> _postingTransferBatch;
    private readonly IPostingBatch<PostingTransferValidation> _postingTransferValidationBatch;
    private readonly IPostingBatch<PostingMerchantBlockage> _postingMerchantBlockageBatch;
    private readonly IPostingBatch<PostingBankBalancer> _postingBankBalancerBatch;
    private readonly IPostingBatch<PostingGrandBalancer> _postingGrandBalancerBatch;
    private readonly IPostingBatch<PostingDeductionCalculator> _postingDeductionCalculatorBatch;
    private readonly IPostingBatch<PostingDeductionBalancer> _postingDeductionBalancerBatch;
    private readonly IPostingBatch<PostingPosBlockage> _postingPosBlockageAccountingBatch;
    private readonly IPostingBatch<PostingParentMerchantBalancer> _postingParentMerchantBalancerBatch;
    private readonly IPostingBatch<PostingDeductionTransfer> _postingDeductionTransferBatch;

    public PostingBatchFactory(IPostingBatch<PostingTransfer> postingTransferBatch,
        IPostingBatch<PostingTransferValidation> postingTransferValidationBatch,
        IPostingBatch<PostingMerchantBlockage> postingMerchantBlockageBatch,
        IPostingBatch<PostingBankBalancer> postingBankBalancerBatch,
        IPostingBatch<PostingGrandBalancer> postingGrandBalancerBatch,
        IPostingBatch<PostingDeductionCalculator> postingDeductionCalculatorBatch,
        IPostingBatch<PostingDeductionBalancer> postingDeductionBalancerBatch,
        IPostingBatch<PostingPosBlockage> postingPosBlockageAccountingBatch,
        IPostingBatch<PostingParentMerchantBalancer> postingParentMerchantBalancerBatch,
        IPostingBatch<PostingDeductionTransfer> postingDeductionTransferBatch)
    {
        _postingTransferBatch = postingTransferBatch;
        _postingTransferValidationBatch = postingTransferValidationBatch;
        _postingMerchantBlockageBatch = postingMerchantBlockageBatch;
        _postingBankBalancerBatch = postingBankBalancerBatch;
        _postingGrandBalancerBatch = postingGrandBalancerBatch;
        _postingDeductionCalculatorBatch = postingDeductionCalculatorBatch;
        _postingDeductionBalancerBatch = postingDeductionBalancerBatch;
        _postingPosBlockageAccountingBatch = postingPosBlockageAccountingBatch;
        _postingParentMerchantBalancerBatch = postingParentMerchantBalancerBatch;
        _postingDeductionTransferBatch = postingDeductionTransferBatch;
    }

    public async Task TriggerBatchAsync(PostingBatchStatus batchStatus)
    {
        switch (batchStatus.PostingBatchLevel)
        {
            case PostingBatchLevel.Transfer:
                await _postingTransferBatch.ExecuteBatchAsync(batchStatus);
                break;
            case PostingBatchLevel.TransferValidation:
                await _postingTransferValidationBatch.ExecuteBatchAsync(batchStatus);
                break;
            case PostingBatchLevel.MerchantBlockage:
                await _postingMerchantBlockageBatch.ExecuteBatchAsync(batchStatus);
                break;
            case PostingBatchLevel.BankBalancer:
                await _postingBankBalancerBatch.ExecuteBatchAsync(batchStatus);
                break;
            case PostingBatchLevel.PosBlockageAccounting:
                await _postingPosBlockageAccountingBatch.ExecuteBatchAsync(batchStatus);
                break;
            case PostingBatchLevel.GrandBalancer:
                await _postingGrandBalancerBatch.ExecuteBatchAsync(batchStatus);
                break;
            case PostingBatchLevel.DeductionBalancer:
                await _postingDeductionBalancerBatch.ExecuteBatchAsync(batchStatus);
                break;
            case PostingBatchLevel.ParentMerchantBalancer:
                await _postingParentMerchantBalancerBatch.ExecuteBatchAsync(batchStatus);
                break;
            case PostingBatchLevel.DeductionTransfer:
                await _postingDeductionTransferBatch.ExecuteBatchAsync(batchStatus);
                break;
            case PostingBatchLevel.DeductionCalculation:
                await _postingDeductionCalculatorBatch.ExecuteBatchAsync(batchStatus);
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}