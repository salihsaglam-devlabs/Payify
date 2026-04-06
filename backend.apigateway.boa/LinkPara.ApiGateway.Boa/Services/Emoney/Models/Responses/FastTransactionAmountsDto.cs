namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses
{
    public class FastTransactionAmountsDto
    {
        public decimal MostTransactionAmount { get; set; }
        public decimal LastTransactionAmount { get; set; }
        public decimal UserBalanceAmount { get; set; }
    }
}