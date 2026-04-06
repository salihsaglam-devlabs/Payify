namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses
{
    public class DeductionDetailsResponse
    {
        public MerchantDeductionDto MerchantDeduction { get; set; }
        public MerchantTransactionDto MerchantTransaction { get; set; }
        public MerchantDueDto MerchantDue { get; set; }
        public List<DeductionTransactionDto> Transactions { get; set; }
        public List<MerchantDeductionDto> RelatedDeductions { get; set; }
    }
}
