namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class PfReportDto
{
    public int PendingApplication { get; set; }
    public int PendingRiskApprovalApplication { get; set; }
    public int InProgressApplication { get; set; }
    public int ActiveMerchant { get; set; }
    public int TotalActiveMerchant { get; set; }
    public int ClosedMerchant { get; set; }
    public int TotalClosedMerchant { get; set; }
    public int RejectedMerchant { get; set; }
    public int TotalRejectedMerchant { get; set; }
    public int SucceededProvision { get; set; }
    public decimal SucceededProvisionAmount { get; set; }
    public int FailedProvision { get; set; }
    public decimal FailedProvisionAmount { get; set; }
    public int SucceededReturn { get; set; }
    public decimal SucceededReturnAmount { get; set; }
    public int FailedReturn { get; set; }
    public decimal FailedReturnAmount { get; set; }
    public int SucceededCancel { get; set; }
    public decimal SucceededCancelAmount { get; set; }
    public int FailedCancel { get; set; }
    public decimal FailedCancelAmount { get; set; }
    public int Chargeback { get; set; }
    public int SuspeciousPayment { get; set; }
    public int ActionRequired { get; set; }
    public int NoEndOfDayTransaction { get; set; }
    public int TotalTimeoutProcessCount { get; set; }
    public string UnhealtyBankNames { get; set; }
    public int ManualReturnCount { get; set; }

}
