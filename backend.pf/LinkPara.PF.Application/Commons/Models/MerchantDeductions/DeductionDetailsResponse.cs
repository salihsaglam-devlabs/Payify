using LinkPara.PF.Application.Commons.Models.DeductionTransactions;
using LinkPara.PF.Application.Features.MerchantDues;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.PostingBalances;

namespace LinkPara.PF.Application.Commons.Models.MerchantDeductions
{
    public class DeductionDetailsResponse
    {
        public MerchantDeductionDto MerchantDeduction { get; set; }
        public MerchantTransactionDto MerchantTransaction { get; set; }
        public MerchantDueDto MerchantDue { get; set; }
        public PostingBalanceDto PostingBalance { get; set; }
        public List<DeductionTransactionDto> Transactions { get; set; }
        public List<MerchantDeductionDto> RelatedDeductions { get; set; }
    }
}
