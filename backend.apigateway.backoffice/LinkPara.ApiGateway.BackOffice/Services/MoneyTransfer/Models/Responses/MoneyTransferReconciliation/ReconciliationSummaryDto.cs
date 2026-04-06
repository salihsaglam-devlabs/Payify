using LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.MoneyTransfer.Models.Responses.MoneyTransferReconciliation
{
    public class ReconciliationSummaryDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public int BankCode { get; set; }
        public string BankName { get; set; }
        public decimal BankFirstBalance { get; set; }
        public decimal BankIncomingTotal { get; set; }
        public decimal BankOutgoingTotal { get; set; }
        public decimal BankChargeTotal { get; set; }
        public decimal BankLastBalance { get; set; }
        public decimal MoneyTransferIncomingTotal { get; set; }
        public decimal MoneyTransferOutgoingTotal { get; set; }
        public decimal MoneyTransferChargeTotal { get; set; }
        public ReconciliationSummaryStatus Status { get; set; }
    }
}
