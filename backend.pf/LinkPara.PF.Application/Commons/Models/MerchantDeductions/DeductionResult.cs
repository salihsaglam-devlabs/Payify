using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Models.MerchantDeductions;

public class DeductionResult
{
    public List<DeductionTransaction> DeductionTransactions { get; set; }
    public List<PostingAdditionalTransaction> PostingAdditionalTransactions { get; set; }
    public List<MerchantDeduction> MerchantDeductions { get; set; }
    public List<PostingBalance> PostingBalances { get; set; }
}